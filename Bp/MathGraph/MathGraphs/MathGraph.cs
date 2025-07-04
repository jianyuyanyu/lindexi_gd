﻿using System.Diagnostics;
using MathGraphs.Serialization;

namespace MathGraphs;

/// <summary>
/// 表示数学上的图结构，包含元素（点）和边的关系
/// </summary>
/// <typeparam name="TElementInfo">元素（点）的数据类型</typeparam>
/// <typeparam name="TEdgeInfo">边的数据类型</typeparam>
public class MathGraph<TElementInfo, TEdgeInfo> : ISerializableElement
{
    /// <summary>
    /// 创建一个空的数学图
    /// </summary>
    public MathGraph()
    {
        _elementList = [];
    }

    private readonly List<MathGraphElement<TElementInfo, TEdgeInfo>> _elementList;

    public IReadOnlyList<MathGraphElement<TElementInfo, TEdgeInfo>> ElementList => _elementList;

    /// <summary>
    /// 创建和将元素（点）加入到图中。此时元素还没与其他元素建立关系，即没有边的出度和入度的关系
    /// </summary>
    /// <param name="value"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public MathGraphElement<TElementInfo, TEdgeInfo> CreateAndAddElement(TElementInfo value, string? id = null)
    {
        var element = new MathGraphElement<TElementInfo, TEdgeInfo>(this, value, id);
        _elementList.Add(element);
        return element;
    }

    /// <summary>
    /// 获取图的序列化器
    /// </summary>
    /// <param name="deserializationContext"></param>
    /// <returns></returns>
    public MathGraphSerializer<TElementInfo, TEdgeInfo> GetSerializer(IDeserializationContext? deserializationContext = null) =>
        new MathGraphSerializer<TElementInfo, TEdgeInfo>(this, deserializationContext);

    /// <summary>
    /// 添加单向边，添加边的同时，会添加元素关系
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="edgeInfo"></param>
    public void AddEdge(MathGraphElement<TElementInfo, TEdgeInfo> from, MathGraphElement<TElementInfo, TEdgeInfo> to,
        TEdgeInfo? edgeInfo = default)
    {
        from.AddOutElement(to);
        Debug.Assert(to.InElementList.Contains(from));

        if (edgeInfo != null)
        {
            var edge = new MathGraphUnidirectionalEdge<TElementInfo, TEdgeInfo>(from, to)
            {
                EdgeInfo = edgeInfo
            };
            from.AddEdge(edge);
        }
    }

    /// <summary>
    /// 添加双向边
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="edgeInfo"></param>
    public void AddBidirectionalEdge(MathGraphElement<TElementInfo, TEdgeInfo> a,
        MathGraphElement<TElementInfo, TEdgeInfo> b, TEdgeInfo? edgeInfo = default)
    {
        AddEdge(a, b);
        AddEdge(b, a);

        if (edgeInfo != null)
        {
            var edge = new MathGraphBidirectionalEdge<TElementInfo, TEdgeInfo>(a, b)
            {
                EdgeInfo = edgeInfo
            };
            a.AddEdge(edge);
            //b.AddEdge(edge);
        }
    }

    #region Serialize

    string ISerializableElement.Serialize()
    {
        var mathGraphSerializer = GetSerializer();
        return mathGraphSerializer.Serialize();
    }

    internal void StartDeserialize(int elementCount)
    {
        _elementList.Clear();
        EnsureElementCapacity(elementCount);
    }

    /// <summary>
    /// 序列化使用设置元素的大小
    /// </summary>
    /// <param name="capacity"></param>
    private void EnsureElementCapacity(int capacity)
    {
        _elementList.EnsureCapacity(capacity);
    }
    #endregion
}