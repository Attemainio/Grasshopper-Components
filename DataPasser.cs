using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Cheetah.Ensembles.Data_Refinement.Components
{
    public class GH_DataHolderComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_DataDamComponent class.
        /// </summary>
        public GH_DataPasserComponent()
          : base("Data Passer", "DP",
              "This component holds the data and passes it forward only if pass is true",
              "Custom", "Data")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Data to store", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Pass", "P", "True, if data is passed forward", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Stored data", GH_ParamAccess.item);
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

                foreach (IGH_Param recipient in base.Params.Output[0].Recipients)
                    recipient.ExpireSolution(recompute: true);
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
        protected override System.Drawing.Bitmap Icon => Cheetah.Properties.Resources.GH_DataHolder.ToBitmap();

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("90EB3F13-53F0-4EF1-89C8-98E76802BD54"); }
        }
    }
}
