using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace MatBlazor
{
    /// <summary>
    ///     Displays a list of validation messages for a specified field within a cascaded <see cref="EditContext" />.
    /// </summary>
    public class MatValidationMessage<TValue> : ComponentBase, IDisposable
    {
        private readonly EventHandler<ValidationStateChangedEventArgs> _validationStateChangedHandler;
        private FieldIdentifier _fieldIdentifier;
        private EditContext _previousEditContext;
        private Expression<Func<TValue>> _previousFieldAccessor;

        /// <summary>
        ///     `
        ///     Constructs an instance of <see cref="ValidationMessage{TValue}" />.
        /// </summary>
        public MatValidationMessage()
        {
            _validationStateChangedHandler = (sender, eventArgs) => StateHasChanged();
        }

        /// <summary>
        ///     Gets or sets a collection of additional attributes that will be applied to the created <c>div</c> element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [CascadingParameter] private EditContext CurrentEditContext { get; set; }

        /// <summary>
        ///     Specifies the field for which validation messages should be displayed.
        /// </summary>
        [Parameter]
        public Expression<Func<TValue>> For { get; set; }

        /// <summary>
        ///     Id of input field or other for accessibility of validation message
        /// </summary>
        [Parameter]
        public string ForId { get; set; }

        [Parameter] public string Id { get; set; }

        void IDisposable.Dispose()
        {
            DetachValidationStateChangedListener();
            Dispose(true);
        }

        /// <inheritdoc />
        protected override void OnParametersSet()
        {
            if (CurrentEditContext == null)
                throw new InvalidOperationException($"{GetType()} requires a cascading parameter " +
                                                    $"of type {nameof(EditContext)}. For example, you can use {GetType()} inside " +
                                                    $"an {nameof(EditForm)}.");

            if (For == null) // Not possible except if you manually specify T
            {
                throw new InvalidOperationException($"{GetType()} requires a value for the " +
                                                    $"{nameof(For)} parameter.");
            }

            if (For != _previousFieldAccessor)
            {
                _fieldIdentifier = FieldIdentifier.Create(For);
                _previousFieldAccessor = For;
            }

            if (CurrentEditContext != _previousEditContext)
            {
                DetachValidationStateChangedListener();
                CurrentEditContext.OnValidationStateChanged += _validationStateChangedHandler;
                _previousEditContext = CurrentEditContext;
            }
        }

        /// <inheritdoc />
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            foreach (var message in CurrentEditContext.GetValidationMessages(_fieldIdentifier))
            {
                builder.OpenElement(0, "p");
                builder.AddMultipleAttributes(1, AdditionalAttributes);
                builder.AddAttribute(2, "id", Id);
                builder.AddAttribute(2, "class", "field-validation-error mdc-text-field-helper-text mdc-text-field-helper-text--persistent mdc-text-field-helper-text--validation-msg");
                builder.AddAttribute(2, "data-valmsg-for", ForId);
                builder.AddContent(3, message);
                builder.CloseElement();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        private void DetachValidationStateChangedListener()
        {
            if (_previousEditContext != null)
                _previousEditContext.OnValidationStateChanged -= _validationStateChangedHandler;
        }
    }
}