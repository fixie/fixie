using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Should;

namespace Fixie.Samples.Bdd
{
    class adding_two_numbers : IDisposable
    {
        static Calculator calculator;
        static StringBuilder log;
        static int _result;

        given a_logger = () =>
                         {
                             log = new StringBuilder();
                             log.WhereAmI();
                         };

        given a_calculator = () =>
                             {
                                 log.WhereAmI();
                                 calculator = new Calculator();
                             };

        when adding_2_and_3 = () =>
                              {
                                  log.WhereAmI();
                                  _result = calculator.Add(2, 3);
                              };

        then the_result_should_be_5 = () =>
                                      {
                                          log.WhereAmI();
                                          _result.ShouldEqual(5);
                                      };

        after test_completion = () => log.WhereAmI();
        
        public void Dispose()
        {
            log.ShouldHaveLines("a_logger", "a_calculator", "adding_2_and_3", "the_result_should_be_5","test_completion");
        }
    }
}
