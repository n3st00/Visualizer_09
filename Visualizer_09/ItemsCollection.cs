using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Visualizer_09;

public class ItemsCollection
{
    public Dictionary<string, Dictionary<string, object>> NamedItems { get; set; }

    public ItemsCollection()
    {
        NamedItems = new Dictionary<string, Dictionary<string, object>>();
    }
}
