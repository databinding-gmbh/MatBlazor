using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;

namespace MatBlazor
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue">any</typeparam>
    public class BaseMatRadioGroupInternal<TValue> : BaseMatInputComponent<TValue>
    {
        [Parameter] 
        public string Label { get; set; }

        [Parameter]
        public RenderFragment<TValue> ItemTemplate { get; set; }

        [Parameter]
        public IEnumerable<TValue> Items { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>
        ///     Specifies the field for which validation messages should be displayed.
        /// </summary>
        [Parameter]
        public Expression<Func<TValue>> For { get; set; }

        public void SetCurrentValue(TValue value)
        {
            this.CurrentValue = value;
        }
    }
}