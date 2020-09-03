using System;
using System.Collections.Generic;
using System.Text;

namespace Tax
{
    public interface ITaxService
    {
        public double GetTaxAmount(string area , string serviceName, double serviceAmount);
    }

    public class TaxService : ITaxService
    {
        public double GetTaxAmount(string area, string serviceName, double serviceAmount)
        {
            //Currently No Logic but 18% of serviceAmount;
            return 0.18 * serviceAmount;
        }
    }
}
