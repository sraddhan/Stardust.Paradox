﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stardust.Paradox.Data.Annotations
{
	public interface IEdgeCollection
	{
		Task LoadAsync();
		Task SaveChangesAsync();
	}
	public interface IEdgeCollection<TTout> : IEdgeCollection, ICollection<IEdge<TTout>>, ICollection<TTout> where TTout : IVertex
	{
		Task<IEnumerable<TTout>> ToVerticesAsync();

		Task<IEnumerable<IEdge<TTout>>> ToEdgesAsync();
		void Add(TTout vertex, IDictionary<string, object> edgeProperties);
		void AddDual(TTout vertex);


	}

	public interface IEdgeCollection<TIn, TOut> : IEdgeCollection, ICollection<IEdge<TIn, TOut>> where TIn : IVertex where TOut : IVertex
	{
		Task<IEnumerable<TIn>> InVerticesAsync();

		Task<IEnumerable<TOut>> OutVerticesAsync();

		Task<IEnumerable<IEdge<TIn, TOut>>> AsEnumerableAsync();
	}

	
}