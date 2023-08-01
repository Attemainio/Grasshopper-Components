using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Ramboll;
using System;
using System.Collections.Generic;

namespace DataRefinement
{
    public class GH_DataRecorderComponent : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the ListBool class.
        /// </summary>
        public GH_DataRecorderComponent()
          : base("Data Recorder", "DR",
              "Improved data recorder with options to toggle and clear data externally. moreover, it can store complete structures",
              "Custom", "Data")
        {
        }

        private bool _recordEmpty = false;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Record", "R", "True = ON, False = OFF", GH_ParamAccess.item, true);
            pManager.AddGenericParameter("Data", "D", "Data to record", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Clear", "C", "Clear data if necessary. Button input.", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Limiter", "SL", "(Optional) Defines how many trees are recorded. A new lists always removes the oldest list. 0 means that there is no limit.", GH_ParamAccess.item, 10);

            for (int i = 0; i < pManager.ParamCount; i++)
            {
                pManager[i].Optional = true;
            }
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Recorded Data", "RD", "Recorded Data", GH_ParamAccess.tree);
        }

        private Queue<GH_Structure<IGH_Goo>> _recordedStructure = new Queue<GH_Structure<IGH_Goo>>();
        private bool _clear = false;
        private int _structureLimit = 10;

        protected override void AppendAdditionalComponentMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "Record Empty Data", Menu_RecordEmptyData, true, this._recordEmpty);
        }

        private void Menu_RecordEmptyData(object sender, EventArgs e)
        {
            _recordEmpty = !_recordEmpty;
            this.ExpireSolution(true);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("recordEmptyData", _recordEmpty);
            return base.Write(writer);
        }


        public override bool Read(GH_IReader reader)
        {
            _recordEmpty = reader.GetBoolean("recordEmptyData");
            return base.Read(reader);
        }


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!RambollConnect.Valid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with connection, please try VPN!");
                return;
            }

            bool record = false;
            bool clear = false;
            int limit = 0;

            DA.GetData(0, ref record);
            DA.GetDataTree(1, out GH_Structure<IGH_Goo> tree);
            DA.GetData(2, ref clear);
            DA.GetData(3, ref limit);

            if (this._clear != clear)
            {
                this._clear = clear;
                this._recordedStructure.Clear();
            }
            else if (record)
            {
                if (limit != _structureLimit)
                {
                    this._structureLimit = limit;
                    this._recordedStructure.Clear();
                }

                if (_recordEmpty || (!_recordEmpty && !tree.IsEmpty))
                {
                    this._recordedStructure.Enqueue(tree.Duplicate());

                    if (_structureLimit != 0 && _structureLimit < _recordedStructure.Count)
                    {
                        this._recordedStructure.Dequeue();
                    }
                }
            }

            GH_Structure<IGH_Goo> recordedStructure = new GH_Structure<IGH_Goo>();
            int count = 0;

            foreach (GH_Structure<IGH_Goo> structure in _recordedStructure)
            {
                if (structure.IsEmpty)
                    recordedStructure.EnsurePath(count);
                else
                {
                    int pathCount = structure.PathCount;

                    for (int i = 0; i < pathCount; i++)
                    {
                        GH_Path currentPath = structure.get_Path(i);
                        var branch = structure.get_Branch(currentPath);

                        GH_Path newPath = currentPath.PrependElement(count);

                        recordedStructure.AppendRange((IEnumerable<IGH_Goo>)branch, newPath);
                    }
                }

                count++;
            }

            DA.SetDataTree(0, recordedStructure);
        }

        // Remember to change the icon!
        
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Cheetah.Properties.Resources.GH_DataRecorder.ToBitmap();

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("69D62206-9E97-4CFC-95D0-EE4794634F54");
    }
}
