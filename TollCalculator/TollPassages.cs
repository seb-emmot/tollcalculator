using System;
using System.Collections.Generic;

namespace TollCalculations;

public record TollPassages(DateTime Day, List<TimeSpan> Passages);
