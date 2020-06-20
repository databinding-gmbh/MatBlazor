using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MatBlazor
{
    /// <summary>
    /// The autocomplete is a normal text input enhanced by a panel of suggested options.
    /// </summary>
    /// <typeparam name="TItem">Type of element type.</typeparam>
    public class BaseMatAutocompleteList<TItem> : BaseMatInputComponent<TItem>
    {
        protected ClassMapper WrapperClassMapper = new ClassMapper();
        protected const int DefaultsElementsInPopup = 10;
        private bool isOpened;
        private string stringValue;
        private TItem _value;
        public MatList ListRef;

        protected IEnumerable<MatAutocompleteListItem<TItem>> GetFilteredCollection(
          string searchText)
        {
            return this.Items.Select<TItem, MatAutocompleteListItem<TItem>>((Func<TItem, MatAutocompleteListItem<TItem>>)(x => new MatAutocompleteListItem<TItem>()
            {
                StringValue = this.ComputeStringValue(x),
                Item = x
            })).Where<MatAutocompleteListItem<TItem>>((Func<MatAutocompleteListItem<TItem>, bool>)(x =>
            {
                if (x == null)
                    return false;
                return string.IsNullOrEmpty(searchText) || x.StringValue?.ToLowerInvariant().Contains(searchText.ToLowerInvariant()) == true;
            })).Take<MatAutocompleteListItem<TItem>>(this.NumberOfElementsInPopup ?? 10);
        }

        protected bool IsShowingClearButton => this.ShowClearButton && !string.IsNullOrEmpty(this.StringValue);

        public bool IsOpened
        {
            get => this.isOpened;
            set
            {
                this.isOpened = value;
                this.OnOpenedChanged.InvokeAsync(value);
                this.StateHasChanged();
            }
        }

        /// <summary>Maximum number of elements displayed in the popup</summary>
        [Parameter]
        public int? NumberOfElementsInPopup { get; set; }

        /// <summary>The label of the TextField</summary>
        [Parameter]
        public string Label { get; set; }

        /// <summary>
        /// The Icon displayed as the leading icon for the TextField
        /// </summary>
        [Parameter]
        public string Icon { get; set; }

        /// <summary>The StringValue displayed in the TextField</summary>
        [Parameter]
        public string StringValue
        {
            get => this.stringValue;
            set
            {
                if (stringValue == value)
                    return;
                this.stringValue = value;
                this.OnTextChanged.InvokeAsync(value);
            }
        }

        /// <summary>
        /// The value to be used to pre-select an item from the list
        /// </summary>
        [Parameter]
        public override TItem Value
        {
            get => this._value;
            set
            {
                this.StringValue = BaseMatAutocompleteList<TItem>.EqualValues(this.Value, default(TItem)) ? string.Empty : this.ComputeStringValue(this.Value);
                if (BaseMatAutocompleteList<TItem>.EqualValues(value, this._value))
                    return;
                this._value = value;
                this.ValueChanged.InvokeAsync(this._value);
                this.ListRef?.SetSelectedIndex(0);
            }
        }

        private static bool EqualValues(TItem a1, TItem a2)
        {
            return EqualityComparer<TItem>.Default.Equals(a1, a2);
        }

        [Parameter]
        public string HelperText { get; set; }

        [Parameter]
        public bool HelperTextPersistent { get; set; }

        [Parameter]
        public bool HelperTextValidation { get; set; }
        
        [Parameter]
        public Func<TItem, string> StringSelector { get; set; }

        /// <summary>
        /// ItemTemplate is used to render the elements in the popup if no template is given then the string value of the objects is displayed..
        /// </summary>
        [Parameter]
        public RenderFragment<TItem> ItemTemplate { get; set; }
        

        /// <summary>The collection which should be rendered and filtered</summary>
        [Parameter]
        public virtual IList<TItem> Items { get; set; }

        /// <summary>
        /// If this parameter is true then the style of the textbox is outlined see `MatTextfield`
        /// </summary>
        [Parameter]
        public bool Outlined { get; set; }

        /// <summary>
        /// OnOpenedChanged is fired when the popup dialog is opened or close and the parameter indicates whenever is it open, the default value is false
        /// </summary>
        [Parameter]
        public EventCallback<bool> OnOpenedChanged { get; set; }

        /// <summary>
        /// OnTextChanged is fired when the string value is changed(when an input occurs in the textfield or when an item is selected)
        /// </summary>
        [Parameter]
        public EventCallback<string> OnTextChanged { get; set; }

        /// <summary>
        /// This value indicates if the clear button(using a trailing icon) should be displayed, which can clear the entire text and the selected value), the default value is false
        /// </summary>
        [Parameter]
        public bool ShowClearButton { get; set; }

        /// <summary>
        /// This value indicates if the textfield and the dialog will be or not displayed in the full screen, the default value is false
        /// </summary>
        [Parameter]
        public bool FullWidth { get; set; }

        protected ClassMapper HelperTextClassMapper = new ClassMapper();

        protected void OpenPopup()
        {
            this.IsOpened = true;
        }

        protected void ClosePopup()
        {
            this.IsOpened = false;
        }

        public void OnValueChanged(ChangeEventArgs ev)
        {
            this.StringValue = (string)ev.Value;
            this.StateHasChanged();
        }

        public async void OnKeyDown(KeyboardEventArgs ev)
        {
            try
            {
                if (ev.Key == "ArrowDown" || ev.Key == "ArrowUp")
                {
                    var currentIndex = await this.ListRef.GetSelectedIndex();
                    var nextIndex = ev.Key == "ArrowDown" ? currentIndex++ : currentIndex--;
                    await this.ListRef.SetSelectedIndex(currentIndex);
                }
                else
                {
                    if (ev.Key != "Enter" && ev.Key != "Tab")
                    {
                        return;
                    }
                    var obj = await this.JsInvokeAsync<object>("matBlazor.matList.confirmSelection", (object)this.ListRef.Ref);
                }
            }
            catch (Exception e)
            {
            }
        }

        public void ItemClicked(TItem selectedObject)
        {
            this.Value = selectedObject;
            this.StateHasChanged();
        }

        public void ItemClicked(object selectedObject)
        {
            this.Value = (TItem)selectedObject;
            this.StateHasChanged();
        }

        public void ClearText(MouseEventArgs e)
        {
            this.Value = default;
            this.StringValue = string.Empty;
            this.StateHasChanged();
        }

        public BaseMatAutocompleteList()
        {
            this.WrapperClassMapper.Add<ClassMapper>("mat-autocomplete-list").Add<ClassMapper>("mat-autocomplete-list-wrapper").If<ClassMapper>("mat-autocomplete-list-wrapper-fullwidth", (Func<bool>)(() => this.FullWidth));

            HelperTextClassMapper
                .Add("mdc-text-field-helper-text")
                .If("mdc-text-field-helper-text--persistent", () => HelperTextPersistent)
                .If("mdc-text-field-helper-text--validation-msg", () => HelperTextValidation);
        }

        private string ComputeStringValue(TItem obj)
        {
            Func<TItem, string> customStringSelector = this.StringSelector;
            return (customStringSelector != null ? customStringSelector(obj)?.TrimEnd() : (string)string.Empty);
        }
    }
}
