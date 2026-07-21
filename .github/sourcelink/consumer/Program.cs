using System;
using System.Collections.Generic;
using Wolfgang.Etl.TestKit;

// End-to-end SourceLink "step into" fixture. The debugger sets a breakpoint on
// the line marked STEP_INTO_TARGET and issues a step-into (the F11 a consumer
// would press). If SourceLink is intact the debugger resolves the library's real
// source (from GitHub) at the constructor below, instead of a decompiled
// placeholder. TestExtractor's constructor is a plain, non-async method, which
// makes it a clean and stable step-into target.

IEnumerable<int> items = new[] { 1, 2, 3 };
var extractor = new TestExtractor<int>(items); // STEP_INTO_TARGET
Console.WriteLine(extractor.GetType().FullName);
