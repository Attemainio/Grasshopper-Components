using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace Gatekeeper
{
    public class GH_GatekeeperComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_DataDamComponent class.
        /// </summary>
        public GH_GatekeeperComponent()
          : base("Gatekeeper", "GK",
              "The Gatekeeper component is a powerful tool designed to manage the flow of data within Grasshopper for Rhino. " +
              "It acts as a conditional gate that can prevent data from propagating further in a Grasshopper definition based on a boolean condition, " +
              "without triggering a recomputation of the solution. " +
              "This component is particularly useful when you need to control the data flow based on specific conditions, " +
              "and it ensures a seamless user experience by retaining the previous data state.",
              "Params", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Data", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Pass", "P", "True, if data is passed forward", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Data", GH_ParamAccess.item);
        }

        private bool _pass = false;
        private GH_Structure<IGH_Goo> _data = null;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool Compute = false;

            DA.GetData(1, ref Compute);

            bool DataIsNull = _data == null;
            bool PassIsTurningFalse = !Compute && _pass == true;

            if (PassIsTurningFalse || DataIsNull || Compute)
            {
                DA.GetDataTree(0, out GH_Structure<IGH_Goo> dataTree);

                if (PassIsTurningFalse || DataIsNull)
                    _data = dataTree.ShallowDuplicate();

                DA.SetDataTree(0, Compute ? dataTree : _data);

                if (DataIsNull || Compute)
                {
                    foreach (IGH_Param recipient in base.Params.Output[0].Recipients)
                        recipient.ExpireSolution(recompute: true);
                }
            }
            else
                DA.SetDataTree(0, _data);

            _pass = Compute;
        }

        protected override void ExpireDownStreamObjects()
        {
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Gatekeeper.Properties.Resources.GH_Gatekeeper.ToBitmap();

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F798609B-3A6A-4736-BD18-E59BF1F95D65"); }
        }
    }
}
