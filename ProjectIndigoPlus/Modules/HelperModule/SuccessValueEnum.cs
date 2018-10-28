using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectIndigoPlus.Modules.HelperModule
{
    /// <summary>
    /// Usefull for functions which return a bool but can produce an error that needs to be handled outside the method's scope
    /// </summary>
    enum SuccessValue
    {
        success,
        failure,
        error
    }
}
