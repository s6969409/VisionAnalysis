using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VisionAnalysis
{
    public class TepObjPaser : BaseToolEditParas
    {
        public TepObjPaser(ObservableRangeCollection<Nd> nodes) : base(nodes)
        {
            #region para value default...
            Inputs["Obj"] = new PInput();
            Inputs["PropertyName"] = new PInput() { value = "" };
            Outputs["Output1"] = new POutput() { value = "" };
            #endregion
        }
        #region override BaseToolEditParas member
        public override Action actionProcess => () =>
        {
            base.actionProcess();//read paras

            //process...
            object obj = Inputs["Obj"].value;
            string PropertyName = Inputs["PropertyName"].value as string;
            PropertyInfo[] properties = obj.GetType().GetProperties();
            if (!(obj is string) && properties.Length > 0)
            {
                foreach (var p in properties)
                {
                    object val = p.GetValue(obj);
                    
                }
            }
            Outputs["Output1"].value = properties.FirstOrDefault(p => 
            p.Name == PropertyName
            ).GetValue(obj);
        };
        #endregion
    }
}
