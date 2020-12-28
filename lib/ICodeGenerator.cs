/*
 * File: ICodeGenerator.cs
 * Project: lib
 * Created Date: 28/12/2020
 * Author: Shun Suzuki
 * -----
 * Last Modified: 28/12/2020
 * Modified By: Shun Suzuki (suzuki@hapis.k.u-tokyo.ac.jp)
 * -----
 * Copyright (c) 2020 Hapis Lab. All rights reserved.
 * 
 */

namespace autd_wrapper_generator.lib
{
    internal interface ICodeGenerator
    {
        string GetCommentPrefix();
        string GetFileHeader();
        string GetFileFooter();
        string GetFunctionDefinition(Function func);
    }
}
