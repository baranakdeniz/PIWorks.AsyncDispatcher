using AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.WebApi.Tests
{
    public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
    {
        public InlineAutoMoqDataAttribute(params object[] values)
                    : base(new AutoMoqDataAttribute(), values)
        {
        }
    }
}
