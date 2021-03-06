﻿using System.Linq;
using System.Collections.Generic;

namespace AElf.Kernel.Concurrency
{
	public static class ResourceExtractor
	{
		public static List<Hash> GetResources(this Transaction tx)
		{
			return new List<Hash>(){
				tx.From, tx.To
			};
		}
	}

	public class Grouper : IGrouper
	{
		public List<List<Transaction>> Process(List<Transaction> transactions)
		{
			if (transactions.Count == 0)
			{
				return new List<List<Transaction>>();
			}

			Dictionary<Hash, UnionFindNode> accountUnionSet = new Dictionary<Hash, UnionFindNode>();

			//set up the union find set

			foreach (var tx in transactions)
			{
				UnionFindNode first = null;
				foreach (var hash in tx.GetResources())
				{
					if (!accountUnionSet.TryGetValue(hash, out var node))
					{
						node = new UnionFindNode();
						accountUnionSet.Add(hash, node);
					}
					if (first == null)
					{
						first = node;
					}
					else
					{
						node.Union(first);
					}
				}
			}

			Dictionary<int, List<Transaction>> grouped = new Dictionary<int, List<Transaction>>();

			foreach (var tx in transactions)
			{
				int nodeId = accountUnionSet[tx.From].Find().NodeId;
				if (!grouped.TryGetValue(nodeId, out var group))
				{
					group = new List<Transaction>();
					grouped.Add(nodeId, group);
				}
				group.Add(tx);
			}

			return grouped.Values.ToList();
		}
	}
}
