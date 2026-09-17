using System;
using System.Collections.Generic;
using Eto.Forms;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Bindings;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Controls.Output;

namespace OpenTabletDriver.UX.Controls
{
    public class ControlPanel : Panel
    {
        public ControlPanel()
        {
            outputModeEditor = new();
            filterEditor = new();
            penBindingEditor = new PenBindingEditor();
            auxBindingEditor = new AuxiliaryBindingEditor();
            mouseBindingEditor = new MouseBindingEditor();
            toolEditor = new();
            placeholder = new Placeholder
            {
                Text = "No tablets are detected."
            };
            logView = new();

            nav = new SideNav();
            nav.Add("Output", outputModeEditor);
            nav.Add("Filters", filterEditor);
            nav.Add("Pen", penBindingEditor);
            nav.Add("Auxiliary", auxBindingEditor);
            nav.Add("Mouse", mouseBindingEditor);
            nav.Add("Tools", toolEditor);
            nav.Add("Info", placeholder);
            nav.Add("Console", logView);

            Content = nav;

            outputModeEditor.ProfileBinding.Bind(ProfileBinding);
            penBindingEditor.ProfileBinding.Bind(ProfileBinding);
            auxBindingEditor.ProfileBinding.Bind(ProfileBinding);
            mouseBindingEditor.ProfileBinding.Bind(ProfileBinding);
            filterEditor.StoreCollectionBinding.Bind(ProfileBinding.Child(p => p!.Filters)!);
            toolEditor.StoreCollectionBinding.Bind(App.Current, a => a.Settings.Tools);

            outputModeEditor.SetDisplaySize(DesktopInterop.VirtualScreen?.Displays);

            Log.Output += (_, message) => Application.Instance.AsyncInvoke(() =>
            {
                if (message.Level > LogLevel.Info)
                    nav.Select(logView);
            });
        }

        private readonly SideNav nav;
        private readonly Placeholder placeholder;
        private readonly LogView logView;
        private readonly OutputModeEditor outputModeEditor;
        private readonly BindingEditor penBindingEditor, auxBindingEditor, mouseBindingEditor;
        private readonly List<BindingEditor> wheelBindingEditors = [];
        private readonly PluginSettingStoreCollectionEditor<IPositionedPipelineElement<IDeviceReport>> filterEditor;
        private readonly PluginSettingStoreCollectionEditor<ITool> toolEditor;

        private Profile? profile;

        private Profile? Profile
        {
            set
            {
                this.profile = value;
                this.OnProfileChanged();
            }
            get => this.profile;
        }

        public event EventHandler<EventArgs>? ProfileChanged;

        // ReSharper disable once AsyncVoidMethod
        protected virtual void OnProfileChanged() => Application.Instance.AsyncInvoke(async void () =>
        {
            ProfileChanged?.Invoke(this, EventArgs.Empty);

            var tablet = Profile != null ? await Profile.GetTabletReference() : null;

            OnTabletChanged(tablet);

            if (tablet != null)
            {
                bool switchToOutput = nav.SelectedContent == placeholder;

                SetPageVisibility(placeholder, false);
                SetPageVisibility(outputModeEditor, true);
                SetPageVisibility(filterEditor, true);
                SetPageVisibility(penBindingEditor, true);
                SetPageVisibility(auxBindingEditor, tablet.Properties.Specifications.AuxiliaryButtons != null);

                for (int i = 0; i < wheelBindingEditors.Count; i++)
                    SetPageVisibility(wheelBindingEditors[i], (tablet.Properties.Specifications.Wheels?.Count ?? 0) > i);

                SetPageVisibility(mouseBindingEditor, tablet.Properties.Specifications.MouseButtons != null);
                SetPageVisibility(toolEditor, true);

                if (switchToOutput)
                    nav.Select(outputModeEditor);
            }
            else
            {
                SetPageVisibility(placeholder, true);
                SetPageVisibility(outputModeEditor, false);
                SetPageVisibility(filterEditor, false);
                SetPageVisibility(penBindingEditor, false);
                SetPageVisibility(auxBindingEditor, false);
                foreach (var controlItem in wheelBindingEditors)
                    SetPageVisibility(controlItem, false);
                SetPageVisibility(mouseBindingEditor, false);
                SetPageVisibility(toolEditor, false);

                if (nav.SelectedContent != logView)
                    nav.Select(placeholder);
            }

            SetPageVisibility(logView, true);
        });

        private void OnTabletChanged(TabletReference? tablet)
        {
            int tabletWheels = tablet?.Properties.Specifications.Wheels?.Count ?? 0;
            if (tabletWheels > wheelBindingEditors.Count)
            {
                for (int i = wheelBindingEditors.Count; i < tabletWheels; i++)
                {
                    var wheelBindingEditor = new WheelBindingEditor(i);
                    wheelBindingEditor.ProfileBinding.Bind(ProfileBinding);
                    wheelBindingEditors.Add(wheelBindingEditor);
                    nav.InsertBefore(mouseBindingEditor, $"Wheel {i + 1}", wheelBindingEditor);
                }
            }
        }

        public BindableBinding<ControlPanel, Profile?> ProfileBinding
        {
            get
            {
                return new BindableBinding<ControlPanel, Profile?>(
                    this,
                    c => c.Profile,
                    (c, v) => c.Profile = v,
                    (c, h) => c.ProfileChanged += h,
                    (c, h) => c.ProfileChanged -= h
                );
            }
        }

        private void SetPageVisibility(Control control, bool visible)
        {
            nav.SetVisible(control, visible);
        }
    }
}
