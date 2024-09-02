﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathGraph;

internal class Program
{
    private static void Main(string[] args)
    {
        var mathGraph = new MathGraph<string, string>();
        var a = mathGraph.CreateAndAddElement("a");
        var b = mathGraph.CreateAndAddElement("b");
        var c = mathGraph.CreateAndAddElement("c");

        mathGraph.AddEdge(a, b);
        mathGraph.AddEdge(b, c);
        Debug.Assert(a.OutElementList[0] == b);
        Debug.Assert(b.OutElementList[0] == c);
        Debug.Assert(c.InElementList[0] == b);
        Debug.Assert(b.InElementList[0] == a);
        SerializeDeserialize(mathGraph);

        AddBidirectionalEdge();

        AddEdge();

        Serialize();
    }

    private static void AddBidirectionalEdge()
    {
        var mathGraph = new MathGraph<string, string>();
        var a = mathGraph.CreateAndAddElement("a");
        var b = mathGraph.CreateAndAddElement("b");

        mathGraph.AddBidirectionalEdge(a, b);

        Debug.Assert(a.OutElementList.Contains(b));
        Debug.Assert(b.InElementList.Contains(a));
        Debug.Assert(a.InElementList.Contains(b));
        Debug.Assert(b.OutElementList.Contains(a));
        SerializeDeserialize(mathGraph);
    }

    private static void AddEdge()
    {
        var mathGraph = new MathGraph<string, string>();
        var a = mathGraph.CreateAndAddElement("a");
        var b = mathGraph.CreateAndAddElement("b");

        mathGraph.AddEdge(a, b);

        Debug.Assert(a.OutElementList.Contains(b));
        Debug.Assert(b.InElementList.Contains(a));

        SerializeDeserialize(mathGraph);
    }

    private static void Serialize()
    {
        var mathGraph = new MathGraph<int, string>();

        List<MathGraphElement<int, string>> elementList = [];

        for (int i = 0; i < 10; i++)
        {
            var mathGraphElement = mathGraph.CreateAndAddElement(i);
            elementList.Add(mathGraphElement);
        }

        for (int i = 0; i < 10; i++)
        {
            var mathGraphElement = elementList[i];
            for (int j = 0; j < 10; j++)
            {
                if (i != j)
                {
                    mathGraphElement.AddInElement(elementList[j]);
                }
            }
        }

        SerializeDeserialize(mathGraph);
    }

    private static void SerializeDeserialize<TElementInfo, TEdgeInfo>(MathGraph<TElementInfo, TEdgeInfo> mathGraph)
    {
        var mathGraphSerializer = mathGraph.GetSerializer();
        var json = mathGraphSerializer.Serialize();
        Equals(mathGraph, Deserialize<TElementInfo, TEdgeInfo>(json));
    }

    private static MathGraph<TElementInfo, TEdgeInfo> Deserialize<TElementInfo, TEdgeInfo>(string json)
    {
        var mathGraph = new MathGraph<TElementInfo, TEdgeInfo>();
        var mathGraphSerializer = mathGraph.GetSerializer();
        mathGraphSerializer.Deserialize(json);
        return mathGraph;
    }

    private static void Equals<TElementInfo, TEdgeInfo>(MathGraph<TElementInfo, TEdgeInfo> a, MathGraph<TElementInfo, TEdgeInfo> b)
    {
        Debug.Assert(a.ElementList.Count == b.ElementList.Count);
        for (var i = 0; i < a.ElementList.Count; i++)
        {
            var elementA = a.ElementList[i];
            var elementB = b.ElementList[i];

            ElementEquals(elementA, elementB);

            ElementListEquals(elementA.InElementList, elementB.InElementList);
            ElementListEquals(elementA.OutElementList, elementB.OutElementList);
        }

        static void ElementListEquals(IReadOnlyList<MathGraphElement<TElementInfo, TEdgeInfo>> a, IReadOnlyList<MathGraphElement<TElementInfo, TEdgeInfo>> b)
        {
            Debug.Assert(a.Count == b.Count);
            for (var i = 0; i < a.Count; i++)
            {
                ElementEquals(a[i], b[i]);
            }
        }

        static void ElementEquals(MathGraphElement<TElementInfo, TEdgeInfo> a, MathGraphElement<TElementInfo, TEdgeInfo> b)
        {
            Debug.Assert(EqualityComparer<TElementInfo>.Default.Equals(a.Value, b.Value));
            Debug.Assert(a.Id == b.Id);
        }
    }
}