using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fixie.Internal;

namespace Fixie.Conventions
{
    public class TraitSourceExpression
    {
        readonly Configuration config;

        internal TraitSourceExpression(Configuration configuration)
        {
            config = configuration;
        }

        public TraitSourceExpression Add(TraitSource traitSource)
        {
            config.AddTraitSource(() => traitSource);
            return this;
        }

        public TraitSourceExpression Add(TraitSourceFunc traitSourceFunc)
        {
            config.AddTraitSource(() => new LambdaTraitSource(traitSourceFunc));
            return this;
        }

        public TraitSourceExpression Add<TTraitSource>() where TTraitSource : TraitSource
        {
            config.AddTraitSource(() => Activator.CreateInstance<TTraitSource>());
            return this;
        }


        /// <summary>
        /// We make lambda wrapper for great justice!
        /// </summary>
        class LambdaTraitSource : TraitSource
        {
            TraitSourceFunc traitSourceFunc;
            public LambdaTraitSource(TraitSourceFunc traitSourceFunc)
            {
                this.traitSourceFunc = traitSourceFunc;
            }

            public IEnumerable<Trait> GetTraits(MethodInfo methodInfo)
            {
                return traitSourceFunc(methodInfo);
            }
        }
    }
}
