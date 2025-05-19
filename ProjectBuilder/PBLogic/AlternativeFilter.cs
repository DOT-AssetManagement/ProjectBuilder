using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBLogic
{
    public class ScenarioCommunique
    {
        public bool Commitment = false;
        public bool ProjectsOnly = false;
        public int? District = null;
        public int? MaxPriority = null;
        public bool SingleTreatmentsOnly = false;
        public bool MixAssetBudgets = false;
        public bool StepByStep = false;
        public bool RunOptimization = true;

        public override string ToString()
        {
            return string.Format($@"[Commitment={Commitment}, ProjectsOnly={ProjectsOnly}, SigleTreatmentsOnly={SingleTreatmentsOnly}, District={(District.HasValue? District.Value : 0)}, 
                MaxPriority={MaxPriority}, JoinAssetConstraints={MixAssetBudgets}, StepByStep={StepByStep}, RunOptimization={RunOptimization}]");
        }
    }
}
