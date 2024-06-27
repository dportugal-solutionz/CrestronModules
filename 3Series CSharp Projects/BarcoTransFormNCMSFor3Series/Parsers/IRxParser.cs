using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BarcoTransFormNCMSFor3Series
{
    /// <summary>
    /// An interface for string parsers recevied from CMS
    /// this class should be in a BarcoCMS class upon which the Action has access to the BarcoCMS members and methods.
    /// </summary>
    public interface IRxParser
    {
        /// <summary>
        /// a method that returns true if the Action method should be invoked based on the received string
        /// suggested implementation is to use return Regex.Match(rx,pattern);
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        bool IsMatch(string rx);
        /// <summary>
        /// If the match is true, then this Action should be invoked.
        /// </summary>
        /// <param name="rx"></param>
        void Action(string rx);
    }
}
