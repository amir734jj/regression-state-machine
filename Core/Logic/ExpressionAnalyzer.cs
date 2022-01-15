using Core.Models;

namespace Core.Logic
{
    internal class ExpressionAnalyzer
    {
        /// <summary>
        /// Checks whether A is a subset of B
        /// </summary>
        public static bool IsSubsetOf(
            SubsetExpressionType subsetExpressionType1, object val1,
            SubsetExpressionType subsetExpressionType2, object val2)
        {
            return subsetExpressionType1 switch
            {
                SubsetExpressionType.Equal when subsetExpressionType2 == SubsetExpressionType.Equal => val1.Equals(val2),
                SubsetExpressionType.NotEqual when subsetExpressionType2 == SubsetExpressionType.NotEqual => val1.Equals(val2),
                SubsetExpressionType.Equal when subsetExpressionType2 == SubsetExpressionType.NotEqual => !val1.Equals(val2),
                SubsetExpressionType.NotEqual when subsetExpressionType2 == SubsetExpressionType.Equal => !val1.Equals(val2),
                _ => false
            };
        }
    }
}