﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Documents.Basic
{
#nullable disable
    /// <summary>
    /// Text-only document
    /// </summary>
    public class Document : ADocument
    {
        public string Text { get; set; }

        public override string GetRelevantText() { return Text; }
    }
}