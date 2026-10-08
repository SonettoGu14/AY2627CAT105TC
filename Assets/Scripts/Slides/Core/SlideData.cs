using System;
[Serializable] public class SlideDeckData { public string key, title, subtitle, source, generated; public SlideData[] slides; }
[Serializable] public class SlideData { public int index; public string layout, title, subtitle; public SlideBlock[] blocks; public SlideTable table; public SlideImage image; }
[Serializable] public class SlideBlock { public int level; public string text, kind; }
[Serializable] public class SlideTable { public int columns; public string[] cells; }   // row-major: cells[r*columns+c]
[Serializable] public class SlideImage { public string path; public int w, h; }
