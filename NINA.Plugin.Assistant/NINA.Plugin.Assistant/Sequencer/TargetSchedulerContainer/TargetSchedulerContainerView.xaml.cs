﻿using System.ComponentModel.Composition;
using System.Windows;

namespace Assistant.NINAPlugin.Sequencer {

    [Export(typeof(ResourceDictionary))]
    public partial class TargetSchedulerContainerView {

        public TargetSchedulerContainerView() {
            InitializeComponent();
        }
    }
}
