﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusCenterPRsChecker.ConnectionReferenceTool.Models
{
    public class IterationModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public Attributes Attributes { get; set; }
        public string Url { get; set; }
    }

    public class Attributes
    {
        public DateTime StartDate { get; set; }
        public DateTime FinishDate { get; set; }
        public string TimeFrame { get; set; }
    }
}
