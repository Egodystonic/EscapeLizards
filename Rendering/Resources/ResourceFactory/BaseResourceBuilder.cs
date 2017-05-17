// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 27 10 2014 at 11:44 by Ben Bowen

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ophidian.Losgap.Rendering {
	/// <summary>
	/// Base abstract class for <see cref="IResourceBuilder"/> implementations. Provides some common functionality for resource builders.
	/// </summary>
	/// <typeparam name="TBuilder">The child type of this class.</typeparam>
	/// <typeparam name="TResource">The resource type that this builder produces.</typeparam>
	/// <typeparam name="TInitialData">The initial data type that resources of type <typeparamref name="TResource"/> require.</typeparam>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes",
		Justification = "The class is mostly to be used internally, and therefore does not have to be particularly 'clean' from" +
			"an API point-of-view.")]
	public abstract class BaseResourceBuilder<TBuilder, TResource, TInitialData> : IResourceBuilder
		where TBuilder : IResourceBuilder
		where TResource : BaseResource {
		/// <summary>
		/// The initial data that is currently attributed to this builder (with <see cref="WithInitialData"/>).
		/// </summary>
		protected readonly TInitialData InitialData;
		/// <summary>
		/// The <see cref="ResourceUsage"/> that is currently attributed to this builder (with <see cref="WithUsage"/>).
		/// </summary>
		protected readonly ResourceUsage Usage;
		private readonly object implicitCreationLock = new object();
		private TResource implicitlyCreatedResource = null;

		/// <summary>
		/// Constructs a new base resource builder.
		/// </summary>
		/// <param name="usage">The <see cref="ResourceUsage"/> to attribute to this builder.</param>
		/// <param name="initialData">The initial data to attribute to this builder.</param>
		protected BaseResourceBuilder(ResourceUsage usage, TInitialData initialData) {
			if (!typeof(TResource).GetCustomAttribute<SupportedUsagesAttribute>().PermittedUsages.Contains(usage)) {
				throw new ArgumentException(
					"Usage '" + usage + "' is not supported for resources of type '" + typeof(TResource).Name + "'.", 
					"usage"
				);
			}
			this.Usage = usage;
			this.InitialData = initialData;
		}

		/// <summary>
		/// Sets the <see cref="ResourceUsage"/> of the resultant <see cref="IResource">resource</see>. Not all usage types are 
		/// valid for all resources.
		/// </summary>
		/// <param name="usage">The usage to use/set.</param>
		/// <returns>A new <see cref="IResourceBuilder"/> of identical type with the given <paramref name="usage"/>.</returns>
		public abstract TBuilder WithUsage(ResourceUsage usage);
		/// <summary>
		/// Sets the initial data to use when creating the <see cref="IResource">resource</see>. Resources with a usage of 
		/// <see cref="ResourceUsage.Immutable"/> must have initial data provided.
		/// </summary>
		/// <param name="initialData">The data to set. May be null where applicable.</param>
		/// <returns>A new <see cref="IResourceBuilder"/> of identical type with the given <paramref name="initialData"/>.</returns>
		public abstract TBuilder WithInitialData(TInitialData initialData);
		/// <summary>
		/// Creates a new <see cref="IResource">resource</see> with the supplied builder parameters (such as <see cref="WithUsage"/> and
		/// <see cref="WithInitialData"/> initialData).
		/// </summary>
		/// <returns>A new resource of type <typeparamref name="TResource"/>.</returns>
		public abstract TResource Create();

		internal static void ThrowIfUnsupportedGPUBinding(GPUBindings bindings) {
			if ((bindings & ~typeof(TResource).GetCustomAttribute<SupportedGPUBindingsAttribute>().PermittedBindings) != 0) {
				throw new ArgumentException(
					"One or more GPU binding in the set '" + bindings + "' is not supported for " +
					"resources of type '" + typeof(TResource).Name + "'."
				);
			}
		}

		IResourceBuilder IResourceBuilder.WithUsage(ResourceUsage usage) {
			return WithUsage(usage);
		}
		IResourceBuilder IResourceBuilder.WithInitialData(object data) {
			if (data == null) throw new ArgumentNullException("data");

			if (data is TInitialData) return WithInitialData((TInitialData) data);
			throw new ArgumentException("Can not set initial data of type '" + data.GetType().Name + "' " +
				"for resource of type '" + typeof(TResource).Name + "'.");
		}
		IResource IResourceBuilder.Create() {
			return Create();
		}

		/// <summary>
		/// Gets a <see cref="IResource">resource</see> with the supplied builder parameters
		/// (such as <see cref="WithUsage"/> and <see cref="WithInitialData"/> initialData).
		/// </summary>
		/// <remarks>
		/// The implicit operator always returns the same resource as long as no builder parameters have been changed. This means a new
		/// resource will be created the first time the implicit operator is used, and then that same resource will be returned each
		/// time thereafter.
		/// </remarks>
		/// <param name="operand">The <see cref="IResourceBuilder"/> whose builder parameters should be used. Must not be null.</param>
		/// <returns>A new <typeparamref name="TResource"/>, with values set according to the <paramref name="operand"/>'s 
		/// builder parameters.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
			Justification = "The parameter is checked by the assurance.")]
		public static implicit operator TResource(BaseResourceBuilder<TBuilder, TResource, TInitialData> operand) {
			Assure.NotNull(operand);
			lock (operand.implicitCreationLock) {
				return operand.implicitlyCreatedResource ?? (operand.implicitlyCreatedResource = operand.Create());
			}
		}
	}
}