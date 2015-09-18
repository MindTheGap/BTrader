using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTraderWPF.Brain
{
  public class Instances
  {
    private List<Instance> _instancesList; 

    public Instances(string filePath)
    {
      _instancesList = new List<Instance>();

      ParseInstancesFile(filePath);
    }

    private void ParseInstancesFile(string filePath)
    {
      
    }
  }
}
