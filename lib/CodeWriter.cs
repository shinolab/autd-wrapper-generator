﻿/*
 * File: CodeWriter.cs
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

using System.IO;

namespace autd_wrapper_generator.lib
{
    internal class CodeWriter
    {
        private readonly ICodeGenerator _engine;
        private readonly string _fileName;
        public CodeWriter(ICodeGenerator engine, string fileName)
        {
            _engine = engine;
            _fileName = fileName;
        }

        public void Write(Parser parser)
        {
            using var sw = new StreamWriter(_fileName);
            sw.WriteLine($"{_engine.GetCommentPrefix()} This file is generated by autd_wrapper_generator");
            sw.WriteLine(_engine.GetFileHeader());
            foreach (var func in parser)
                sw.WriteLine(_engine.GetFunctionDefinition(func));
            sw.WriteLine(_engine.GetFileFooter());
        }
    }
}
