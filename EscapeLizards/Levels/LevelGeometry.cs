// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 14 05 2015 at 16:04 by Ben Bowen

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Ophidian.Losgap;
using Ophidian.Losgap.AssetManagement;
using Ophidian.Losgap.CSG;
using Ophidian.Losgap.Entities;
using Ophidian.Losgap.Rendering;

namespace Egodystonic.EscapeLizards {
	public abstract class LevelGeometry {
		public const float GEOMETRY_MASS = Single.PositiveInfinity;
		protected const int NUM_DECIMAL_PLACES = 8;
		protected const string CUBOID_ELEMENT_NAME = "cuboid";
		protected const string CONE_ELEMENT_NAME = "cone";
		protected const string SPHERE_ELEMENT_NAME = "sphere";
		protected const string CURVE_ELEMENT_NAME = "curve";
		protected const string PLANE_ELEMENT_NAME = "plane";
		protected const string MODEL_ELEMENT_NAME = "model";
		private const string TEXTURE_SCALING_ATTRIBUTE_NAME = "texscaling";
		private const string TRANSFORM_ATTRIBUTE_NAME = "transform";
		private const string EULER_ROTATIONS_ATTRIBUTE_NAME = "euler";
		private const string INSIDE_OUT_ATTRIBUTE_NAME = "inverted";
		private const string ID_ATTRIBUTE_NAME = "id";
		private const string SKYDOME_ATTRIBUTE_NAME = "sky";
		public readonly Vector2 TextureScaling;
		public readonly Transform Transform;
		public readonly bool IsInsideOut;
		public readonly Vector3 EulerRotations;
		public readonly int ID;
		public readonly bool IsSkydome;

		protected LevelGeometry(Vector2 textureScaling, Vector3 scale, Vector3 eulerRotations, Vector3 translation, 
			bool isInsideOut, int id) {
			TextureScaling = textureScaling;
			Transform = new Transform(scale, Quaternion.FromEulerRotations(eulerRotations.X, eulerRotations.Y, eulerRotations.Z), translation);
			IsInsideOut = isInsideOut;
			EulerRotations = eulerRotations;
			ID = id;
			IsSkydome = false;
		}

		protected LevelGeometry(XElement xmlElement) {
			TextureScaling = Vector2.Parse(xmlElement.Attribute(TEXTURE_SCALING_ATTRIBUTE_NAME).Value);
			Transform = Transform.Parse(xmlElement.Attribute(TRANSFORM_ATTRIBUTE_NAME).Value);
			IsInsideOut = Boolean.Parse(xmlElement.Attribute(INSIDE_OUT_ATTRIBUTE_NAME).Value);
			EulerRotations = Vector3.Parse(xmlElement.Attribute(EULER_ROTATIONS_ATTRIBUTE_NAME).Value);
			ID = int.Parse(xmlElement.Attribute(ID_ATTRIBUTE_NAME).Value);
			var skydomeAttr = xmlElement.Attribute(SKYDOME_ATTRIBUTE_NAME);
			IsSkydome = skydomeAttr != null && skydomeAttr.Value.ToUpper() == "TRUE";
		}

		public static LevelGeometry Deserialize(XElement xmlElement) {
			switch (xmlElement.Name.ToString()) {
				case CUBOID_ELEMENT_NAME:
					return new LevelGeometry_Cuboid(xmlElement);
				case CONE_ELEMENT_NAME:
					return new LevelGeometry_Cone(xmlElement);
				case SPHERE_ELEMENT_NAME:
					return new LevelGeometry_Sphere(xmlElement);
				case CURVE_ELEMENT_NAME:
					return new LevelGeometry_Curve(xmlElement);
				case PLANE_ELEMENT_NAME:
					return new LevelGeometry_Plane(xmlElement);
				case MODEL_ELEMENT_NAME:
					return new LevelGeometry_Model(xmlElement);
				default:
					throw new FormatException("Unknown geometry type: " + xmlElement.Name);
			}
		}

		public abstract void GetVertexData(out List<DefaultVertex> outVertices, out List<uint> outIndices);
		public abstract XElement Serialize();

		public abstract LevelGeometry Clone(int newID);

		protected List<DefaultVertex> ConvertCSGVertices(IList<CSGVertex> csgVertices) {
			List<DefaultVertex> result = new List<DefaultVertex>();
			for (int tri = 0; tri < csgVertices.Count / 3; ++tri) {
				CSGVertex c1 = csgVertices[tri * 3 + 0];
				CSGVertex c2 = csgVertices[tri * 3 + 1];
				CSGVertex c3 = csgVertices[tri * 3 + 2];
				Vector3 v1 = c1.Position;
				Vector2 w1 = c1.TexUV;
				Vector3 v2 = c2.Position;
				Vector2 w2 = c2.TexUV;
				Vector3 v3 = c3.Position;
				Vector2 w3 = c3.TexUV;
				float x1 = v2.X - v1.X;
				float x2 = v3.X - v1.X;
				float y1 = v2.Y - v1.Y;
				float y2 = v3.Y - v1.Y;
				float z1 = v2.Z - v1.Z;
				float z2 = v3.Z - v1.Z;
				
				float s1 = w2.X - w1.X;
				float s2 = w3.X - w1.X;
				float t1 = w2.Y - w1.Y;
				float t2 = w3.Y - w1.Y;
				
				float r = 1f / (s1 * t2 - s2 * t1);
				Vector3 tan = new Vector3(
					(t2 * x1 - t1 * x2) * r, 
					(t2 * y1 - t1 * y2) * r,
					(t2 * z1 - t1 * z2) * r
				);

				result.Add(new DefaultVertex(c1.Position, c1.Normal, tan, c1.TexUV));
				result.Add(new DefaultVertex(c2.Position, c2.Normal, tan, c2.TexUV));
				result.Add(new DefaultVertex(c3.Position, c3.Normal, tan, c3.TexUV));
			}
			return csgVertices.Select(csg => new DefaultVertex(csg.Position, csg.Normal, csg.TexUV)).ToList();
		}

		protected string GetTransformString() {
			return " || SRT = " + Transform.Scale + " * " + EulerRotations + " * " + Transform.Translation;
		}

		protected void AddGenericSerializationProperties(XElement geometryRootElement) {
			geometryRootElement.SetAttributeValue(TEXTURE_SCALING_ATTRIBUTE_NAME, TextureScaling.ToString(NUM_DECIMAL_PLACES));
			geometryRootElement.SetAttributeValue(TRANSFORM_ATTRIBUTE_NAME, Transform.ToString(NUM_DECIMAL_PLACES));
			geometryRootElement.SetAttributeValue(INSIDE_OUT_ATTRIBUTE_NAME, IsInsideOut);
			geometryRootElement.SetAttributeValue(EULER_ROTATIONS_ATTRIBUTE_NAME, EulerRotations);
			geometryRootElement.SetAttributeValue(ID_ATTRIBUTE_NAME, ID);
			if (IsSkydome) geometryRootElement.SetAttributeValue(SKYDOME_ATTRIBUTE_NAME, "TRUE");
		}

		public abstract string GetShortDesc();

		public abstract PhysicsShapeHandle CreatePhysicsShape(out Vector3 shapeOffset);
	}

	public sealed class LevelGeometry_Cuboid : LevelGeometry {
		private const string WIDTH_ATTRIBUTE_NAME = "width";
		private const string HEIGHT_ATTRIBUTE_NAME = "height";
		private const string DEPTH_ATTRIBUTE_NAME = "depth";
		private const string FRONT_BOTTOM_LEFT_ATTRIBUTE_NAME = "fbl";
		public readonly Cuboid ShapeDesc;

		public LevelGeometry_Cuboid(Vector2 textureScaling, Vector3 scale, Vector3 eulerRotations, Vector3 translation, bool isInsideOut, Cuboid shapeDesc, int id)
			: base(textureScaling, scale, eulerRotations, translation, isInsideOut, id) {
			ShapeDesc = shapeDesc;
		}

		public LevelGeometry_Cuboid(XElement xmlElement) : base(xmlElement) {
			float width = float.Parse(xmlElement.Attribute(WIDTH_ATTRIBUTE_NAME).Value, CultureInfo.InvariantCulture);
			float height = float.Parse(xmlElement.Attribute(HEIGHT_ATTRIBUTE_NAME).Value, CultureInfo.InvariantCulture);
			float depth = float.Parse(xmlElement.Attribute(DEPTH_ATTRIBUTE_NAME).Value, CultureInfo.InvariantCulture);
			Vector3 fbl = Vector3.Parse(xmlElement.Attribute(FRONT_BOTTOM_LEFT_ATTRIBUTE_NAME).Value);

			this.ShapeDesc = new Cuboid(fbl, width, height, depth);
		}

		public override void GetVertexData(out List<DefaultVertex> outVertices, out List<uint> outIndices) {
			IList<CSGVertex> outCSGVertices = new List<CSGVertex>();
			IList<uint> outCSGIndices = new List<uint>();

			MeshGenerator.GenerateCuboid(
				ShapeDesc,
				Transform,
				TextureScaling * PhysicsManager.ONE_METRE_SCALED,
				IsInsideOut,
				out outCSGVertices,
				out outCSGIndices
			);

			outVertices = ConvertCSGVertices(outCSGVertices);
			outIndices = (List<uint>) outCSGIndices;
		}

		public override XElement Serialize() {
			XElement result = new XElement(CUBOID_ELEMENT_NAME);
			result.SetAttributeValue(WIDTH_ATTRIBUTE_NAME, ShapeDesc.Width.ToString(NUM_DECIMAL_PLACES));
			result.SetAttributeValue(HEIGHT_ATTRIBUTE_NAME, ShapeDesc.Height.ToString(NUM_DECIMAL_PLACES));
			result.SetAttributeValue(DEPTH_ATTRIBUTE_NAME, ShapeDesc.Depth.ToString(NUM_DECIMAL_PLACES));
			result.SetAttributeValue(FRONT_BOTTOM_LEFT_ATTRIBUTE_NAME, ShapeDesc.FrontBottomLeft.ToString(NUM_DECIMAL_PLACES));
			AddGenericSerializationProperties(result);
			return result;
		}

		public override LevelGeometry Clone(int newID) {
			return new LevelGeometry_Cuboid(TextureScaling, Transform.Scale, EulerRotations, Transform.Translation, IsInsideOut, ShapeDesc, newID);
		}

		public override string ToString() {
			return "Cuboid || " + ShapeDesc.Width + "x" + ShapeDesc.Height + "x" + ShapeDesc.Depth + GetTransformString();
		}

		public override string GetShortDesc() {
			return "Cuboid [" + ShapeDesc.Width + "x" + ShapeDesc.Height + "x" + ShapeDesc.Depth + "]";
		}

		public override PhysicsShapeHandle CreatePhysicsShape(out Vector3 shapeOffset) {
			shapeOffset = Vector3.ZERO;
			return PhysicsManager.CreateBoxShape(ShapeDesc.Width, ShapeDesc.Height, ShapeDesc.Depth, new CollisionShapeOptionsDesc(Transform.Scale));
		}
	}

	public sealed class LevelGeometry_Sphere : LevelGeometry {
		private const string RADIUS_ATTRIBUTE_NAME = "radius";
		private const string CENTER_ATTRIBUTE_NAME = "center";
		private const string EXTRAPOLATION_ATTRIBUTE_NAME = "extrapolation";
		public readonly Sphere ShapeDesc;
		public readonly uint Extrapolation;

		public LevelGeometry_Sphere(Vector2 textureScaling, Vector3 scale, Vector3 eulerRotations, Vector3 translation, bool isInsideOut, Sphere shapeDesc, uint extrapolation, int id)
			: base(textureScaling, scale, eulerRotations, translation, isInsideOut, id) {
			ShapeDesc = shapeDesc;
			Extrapolation = extrapolation;
		}

		public LevelGeometry_Sphere(XElement xmlElement) : base(xmlElement) {
			float radius = float.Parse(xmlElement.Attribute(RADIUS_ATTRIBUTE_NAME).Value, CultureInfo.InvariantCulture);
			Vector3 center = Vector3.Parse(xmlElement.Attribute(CENTER_ATTRIBUTE_NAME).Value);
			uint extrapolation = uint.Parse(xmlElement.Attribute(EXTRAPOLATION_ATTRIBUTE_NAME).Value);
			
			this.ShapeDesc = new Sphere(center, radius);
			this.Extrapolation = extrapolation;
		}

		public override void GetVertexData(out List<DefaultVertex> outVertices, out List<uint> outIndices) {
			IList<CSGVertex> outCSGVertices = new List<CSGVertex>();
			IList<uint> outCSGIndices = new List<uint>();

			MeshGenerator.GenerateSphere(
				ShapeDesc,
				Transform,
				TextureScaling * PhysicsManager.ONE_METRE_SCALED,
				Extrapolation,
				IsInsideOut,
				out outCSGVertices,
				out outCSGIndices
			);

			outVertices = ConvertCSGVertices(outCSGVertices);
			outIndices = (List<uint>) outCSGIndices;
		}

		public override XElement Serialize() {
			XElement result = new XElement(SPHERE_ELEMENT_NAME);
			result.SetAttributeValue(CENTER_ATTRIBUTE_NAME, ShapeDesc.Center.ToString(NUM_DECIMAL_PLACES));
			result.SetAttributeValue(RADIUS_ATTRIBUTE_NAME, ShapeDesc.Radius.ToString(NUM_DECIMAL_PLACES));
			result.SetAttributeValue(EXTRAPOLATION_ATTRIBUTE_NAME, Extrapolation.ToString());
			AddGenericSerializationProperties(result);
			return result;
		}

		public override LevelGeometry Clone(int newID) {
			return new LevelGeometry_Sphere(TextureScaling, Transform.Scale, EulerRotations, Transform.Translation, IsInsideOut, ShapeDesc, Extrapolation, newID);
		}

		public override string ToString() {
			return "Sphere || " + ShapeDesc.Radius + " Radius, " + Extrapolation + " extrapolation" + GetTransformString();
		}

		public override string GetShortDesc() {
			return "Sphere [" + ShapeDesc.Radius + "r " + Extrapolation + "e]";
		}

		public override PhysicsShapeHandle CreatePhysicsShape(out Vector3 shapeOffset) {
			shapeOffset = Vector3.ZERO;
			if (Transform.Scale.EqualsExactly(Vector3.ONE)) return PhysicsManager.CreateSimpleSphereShape(ShapeDesc.Radius, new CollisionShapeOptionsDesc(Transform.Scale));
			else return PhysicsManager.CreateScaledSphereShape(ShapeDesc.Radius, Transform.Scale, new CollisionShapeOptionsDesc(Transform.Scale));
		}
	}

	public sealed class LevelGeometry_Cone : LevelGeometry {
		private const string TOP_RADIUS_ATTRIBUTE_NAME = "topRadius";
		private const string BOTTOM_RADIUS_ATTRIBUTE_NAME = "bottomRadius";
		private const string HEIGHT_ATTRIBUTE_NAME = "height";
		private const string TOP_CENTER_ATTRIBUTE_NAME = "topCenter";
		private const string NUM_SIDES_ATTRIBUTE_NAME = "numSides";
		public readonly Cone ShapeDesc;
		public readonly uint NumSides;

		public LevelGeometry_Cone(Vector2 textureScaling, Vector3 scale, Vector3 eulerRotations, Vector3 translation, bool isInsideOut, Cone shapeDesc, uint numSides, int id)
			: base(textureScaling, scale, eulerRotations, translation, isInsideOut, id) {
			ShapeDesc = shapeDesc;
			NumSides = numSides;
		}

		public LevelGeometry_Cone(XElement xmlElement) : base(xmlElement) {
			float topRadius = float.Parse(xmlElement.Attribute(TOP_RADIUS_ATTRIBUTE_NAME).Value, CultureInfo.InvariantCulture);
			float bottomRadius = float.Parse(xmlElement.Attribute(BOTTOM_RADIUS_ATTRIBUTE_NAME).Value, CultureInfo.InvariantCulture);
			float height = float.Parse(xmlElement.Attribute(HEIGHT_ATTRIBUTE_NAME).Value, CultureInfo.InvariantCulture);
			uint numSides = uint.Parse(xmlElement.Attribute(NUM_SIDES_ATTRIBUTE_NAME).Value);
			Vector3 topCenter = Vector3.Parse(xmlElement.Attribute(TOP_CENTER_ATTRIBUTE_NAME).Value);

			ShapeDesc = new Cone(topCenter, bottomRadius, height, topRadius);
			NumSides = numSides;
		}

		public override void GetVertexData(out List<DefaultVertex> outVertices, out List<uint> outIndices) {
			IList<CSGVertex> outCSGVertices = new List<CSGVertex>();
			IList<uint> outCSGIndices = new List<uint>();

			MeshGenerator.GenerateCone(
				ShapeDesc,
				Transform,
				TextureScaling * PhysicsManager.ONE_METRE_SCALED,
				NumSides,
				IsInsideOut,
				out outCSGVertices,
				out outCSGIndices
			);

			outVertices = ConvertCSGVertices(outCSGVertices);
			outIndices = (List<uint>) outCSGIndices;
		}

		public override XElement Serialize() {
			XElement result = new XElement(CONE_ELEMENT_NAME);
			result.SetAttributeValue(TOP_RADIUS_ATTRIBUTE_NAME, ShapeDesc.TopRadius.ToString(NUM_DECIMAL_PLACES));
			result.SetAttributeValue(BOTTOM_RADIUS_ATTRIBUTE_NAME, ShapeDesc.BottomRadius.ToString(NUM_DECIMAL_PLACES));
			result.SetAttributeValue(HEIGHT_ATTRIBUTE_NAME, ShapeDesc.Height.ToString(NUM_DECIMAL_PLACES));
			result.SetAttributeValue(NUM_SIDES_ATTRIBUTE_NAME, NumSides);
			result.SetAttributeValue(TOP_CENTER_ATTRIBUTE_NAME, ShapeDesc.TopCenter.ToString(NUM_DECIMAL_PLACES));
			AddGenericSerializationProperties(result);
			return result;
		}

		public override LevelGeometry Clone(int newID) {
			return new LevelGeometry_Cone(TextureScaling, Transform.Scale, EulerRotations, Transform.Translation, IsInsideOut, ShapeDesc, NumSides, newID);
		}

		public override string ToString() {
			return (Math.Abs(ShapeDesc.TopRadius - ShapeDesc.BottomRadius) < MathUtils.FlopsErrorMargin ? "Cylinder" : "Cone")
				+ " || " + ShapeDesc.TopRadius + " & " + ShapeDesc.BottomRadius + " radii, " + ShapeDesc.Height + " height" + GetTransformString();
		}

		public override string GetShortDesc() {
			return (
				Math.Abs(ShapeDesc.TopRadius - ShapeDesc.BottomRadius) < MathUtils.FlopsErrorMargin ?
				"Cylinder [" + ShapeDesc.TopRadius + "x" + ShapeDesc.Height + "]" 
				:
				"Cone [" + ShapeDesc.TopRadius + "&" + ShapeDesc.BottomRadius + "x" + ShapeDesc.Height + "]"
			);
		}

		public override PhysicsShapeHandle CreatePhysicsShape(out Vector3 shapeOffset) {
			if (Math.Abs(ShapeDesc.TopRadius - ShapeDesc.BottomRadius) < MathUtils.FlopsErrorMargin) {
				return PhysicsManager.CreateCylinderShape(ShapeDesc.BottomRadius, ShapeDesc.Height, new CollisionShapeOptionsDesc(Transform.Scale), out shapeOffset);
			}
			else if (Math.Abs(ShapeDesc.TopRadius) < MathUtils.FlopsErrorMargin) {
				return PhysicsManager.CreateConeShape(ShapeDesc.BottomRadius, ShapeDesc.Height, new CollisionShapeOptionsDesc(Transform.Scale), out shapeOffset);
			}
			else {
				List<DefaultVertex> outVerts;
				List<uint> outIndices;
				GetVertexData(out outVerts, out outIndices);
				shapeOffset = Vector3.ZERO;
				return PhysicsManager.CreateConcaveHullShape(
					outVerts.Select(df => df.Position), 
					outIndices.Select(@uint => (int) @uint), 
					new CollisionShapeOptionsDesc(Transform.Scale),
					null
				);
			}
		}
	}

	public sealed class LevelGeometry_Curve : LevelGeometry {
		private const string WIDTH_ATTRIBUTE_NAME = "width";
		private const string HEIGHT_ATTRIBUTE_NAME = "height";
		private const string DEPTH_ATTRIBUTE_NAME = "depth";
		private const string FRONT_BOTTOM_LEFT_ATTRIBUTE_NAME = "fbl";
		private const string YAW_ROTATION_ATTRIBUTE_NAME = "yawRot";
		private const string PITCH_ROTATION_ATTRIBUTE_NAME = "pitchRot";
		private const string ROLL_ROTATION_ATTRIBUTE_NAME = "rollRot";
		private const string NUM_SEGMENTS_ATTRIBUTE_NAME = "numSegments";
		public readonly MeshGenerator.CurveDesc ShapeDesc;

		public LevelGeometry_Curve(Vector2 textureScaling, Vector3 scale, Vector3 eulerRotations, Vector3 translation, bool isInsideOut, MeshGenerator.CurveDesc shapeDesc, int id)
			: base(textureScaling, scale, eulerRotations, translation, isInsideOut, id) {
			ShapeDesc = shapeDesc;
		}

		public LevelGeometry_Curve(XElement xmlElement)
			: base(xmlElement) {
			float width = float.Parse(xmlElement.Attribute(WIDTH_ATTRIBUTE_NAME).Value, CultureInfo.InvariantCulture);
			float height = float.Parse(xmlElement.Attribute(HEIGHT_ATTRIBUTE_NAME).Value, CultureInfo.InvariantCulture);
			float depth = float.Parse(xmlElement.Attribute(DEPTH_ATTRIBUTE_NAME).Value, CultureInfo.InvariantCulture);
			Vector3 fbl = Vector3.Parse(xmlElement.Attribute(FRONT_BOTTOM_LEFT_ATTRIBUTE_NAME).Value);
			float yawRot = float.Parse(xmlElement.Attribute(YAW_ROTATION_ATTRIBUTE_NAME).Value, CultureInfo.InvariantCulture);
			float pitchRot = float.Parse(xmlElement.Attribute(PITCH_ROTATION_ATTRIBUTE_NAME).Value, CultureInfo.InvariantCulture);
			float rollRot = float.Parse(xmlElement.Attribute(ROLL_ROTATION_ATTRIBUTE_NAME).Value, CultureInfo.InvariantCulture);
			uint numSegments = uint.Parse(xmlElement.Attribute(NUM_SEGMENTS_ATTRIBUTE_NAME).Value, CultureInfo.InvariantCulture);

			this.ShapeDesc = new MeshGenerator.CurveDesc(
				new Cuboid(fbl, width, height, depth), 
				yawRot, pitchRot, rollRot,
				numSegments
			);
		}

		public override void GetVertexData(out List<DefaultVertex> outVertices, out List<uint> outIndices) {
			IList<CSGVertex> outCSGVertices = new List<CSGVertex>();
			IList<uint> outCSGIndices = new List<uint>();

			MeshGenerator.GenerateCurve(
				ShapeDesc,
				Transform,
				TextureScaling * PhysicsManager.ONE_METRE_SCALED,
				IsInsideOut,
				out outCSGVertices,
				out outCSGIndices
			);

			outVertices = ConvertCSGVertices(outCSGVertices);
			outIndices = (List<uint>) outCSGIndices;
		}

		public override XElement Serialize() {
			XElement result = new XElement(CURVE_ELEMENT_NAME);
			result.SetAttributeValue(WIDTH_ATTRIBUTE_NAME, ShapeDesc.StartingCuboid.Width.ToString(NUM_DECIMAL_PLACES));
			result.SetAttributeValue(HEIGHT_ATTRIBUTE_NAME, ShapeDesc.StartingCuboid.Height.ToString(NUM_DECIMAL_PLACES));
			result.SetAttributeValue(DEPTH_ATTRIBUTE_NAME, ShapeDesc.StartingCuboid.Depth.ToString(NUM_DECIMAL_PLACES));
			result.SetAttributeValue(FRONT_BOTTOM_LEFT_ATTRIBUTE_NAME, ShapeDesc.StartingCuboid.FrontBottomLeft.ToString(NUM_DECIMAL_PLACES));
			result.SetAttributeValue(YAW_ROTATION_ATTRIBUTE_NAME, ShapeDesc.YawRotationRads.ToString(NUM_DECIMAL_PLACES));
			result.SetAttributeValue(PITCH_ROTATION_ATTRIBUTE_NAME, ShapeDesc.PitchRotationRads.ToString(NUM_DECIMAL_PLACES));
			result.SetAttributeValue(ROLL_ROTATION_ATTRIBUTE_NAME, ShapeDesc.RollRotationRads.ToString(NUM_DECIMAL_PLACES));
			result.SetAttributeValue(NUM_SEGMENTS_ATTRIBUTE_NAME, ShapeDesc.NumSegments.ToString());
			AddGenericSerializationProperties(result);
			return result;
		}

		public override LevelGeometry Clone(int newID) {
			return new LevelGeometry_Curve(TextureScaling, Transform.Scale, EulerRotations, Transform.Translation, IsInsideOut, ShapeDesc, newID);
		}

		public override string ToString() {
			return "Curve || " 
				+ ShapeDesc.StartingCuboid.Width + "x" + ShapeDesc.StartingCuboid.Width + "x" + ShapeDesc.StartingCuboid.Width
				+ " --> YPW " + MathUtils.RadToDeg(ShapeDesc.YawRotationRads).ToString(0) + "°, " + MathUtils.RadToDeg(ShapeDesc.PitchRotationRads).ToString(0) + "°, " + MathUtils.RadToDeg(ShapeDesc.RollRotationRads).ToString(0)
				+ " in " + ShapeDesc.NumSegments + " segs"
				+ GetTransformString();
		}

		public override string GetShortDesc() {
			return "Curve ["
				+ MathUtils.RadToDeg(ShapeDesc.YawRotationRads).ToString(0) + "°, "
				+ MathUtils.RadToDeg(ShapeDesc.PitchRotationRads).ToString(0) + "°, "
				+ MathUtils.RadToDeg(ShapeDesc.RollRotationRads).ToString(0) + "° " 
			+ "]";
		}

		public override PhysicsShapeHandle CreatePhysicsShape(out Vector3 shapeOffset) {
			var trapPrismVerts = MeshGenerator.GetTrapezoidalPrismVertices(ShapeDesc, Transform);
			shapeOffset = Vector3.ZERO;
			return PhysicsManager.CreateCompoundCurveShape(trapPrismVerts, new CollisionShapeOptionsDesc(Transform.Scale));
		}
	}

	public sealed class LevelGeometry_Plane : LevelGeometry {
		private const string PLANE_CENTER_POINT_ATTRIBUTE_NAME = "centerPoint";
		private const string PLANE_NORMAL_ATTRIBUTE_NAME = "normal";
		private const string CORNER_ELEMENT_NAME = "corner";
		private const string CORNER_POSITION_ATTRIBUTE_NAME = "position";
		public readonly Plane ShapeDesc;
		public readonly IReadOnlyList<Vector3> Corners;

		public LevelGeometry_Plane(Vector2 textureScaling, Vector3 scale, Vector3 eulerRotations, Vector3 translation, bool isInsideOut, IList<Vector3> corners, int id)
			: base(textureScaling, scale, eulerRotations, translation, isInsideOut, id) {

			ShapeDesc = Plane.FromPoints(corners[0], corners[1], corners[2]);
			Corners = new ReadOnlyCollection<Vector3>(corners);
		}

		public LevelGeometry_Plane(XElement xmlElement) : base(xmlElement) {
			Vector3 centerPoint = Vector3.Parse(xmlElement.Attribute(PLANE_CENTER_POINT_ATTRIBUTE_NAME).Value);
			Vector3 normal = Vector3.Parse(xmlElement.Attribute(PLANE_NORMAL_ATTRIBUTE_NAME).Value);
			
			ShapeDesc = new Plane(normal, centerPoint);
			Corners = new ReadOnlyCollection<Vector3>(xmlElement
				.Elements(CORNER_ELEMENT_NAME)
				.Select(cornerElement => Vector3.Parse(cornerElement.Attribute(CORNER_POSITION_ATTRIBUTE_NAME).Value))
				.ToList());
		}

		public override void GetVertexData(out List<DefaultVertex> outVertices, out List<uint> outIndices) {
			IList<CSGVertex> outCSGVertices = new List<CSGVertex>();
			IList<uint> outCSGIndices = new List<uint>();

			MeshGenerator.GeneratePlane(
				(IList<Vector3>) Corners,
				Transform,
				TextureScaling * PhysicsManager.ONE_METRE_SCALED,
				IsInsideOut,
				out outCSGVertices,
				out outCSGIndices
			);

			outVertices = ConvertCSGVertices(outCSGVertices);
			outIndices = (List<uint>) outCSGIndices;
		}

		public override XElement Serialize() {
			XElement result = new XElement(PLANE_ELEMENT_NAME);
			result.SetAttributeValue(PLANE_CENTER_POINT_ATTRIBUTE_NAME, ShapeDesc.CentrePoint.ToString(NUM_DECIMAL_PLACES));
			result.SetAttributeValue(PLANE_NORMAL_ATTRIBUTE_NAME, ShapeDesc.Normal.ToString(NUM_DECIMAL_PLACES));
			foreach (Vector3 corner in Corners) {
				XElement cornerElement = new XElement(CORNER_ELEMENT_NAME);
				cornerElement.SetAttributeValue(CORNER_POSITION_ATTRIBUTE_NAME, corner.ToString(NUM_DECIMAL_PLACES));
				result.Add(cornerElement);
			}
			AddGenericSerializationProperties(result);
			return result;
		}

		public override LevelGeometry Clone(int newID) {
			return new LevelGeometry_Plane(TextureScaling, Transform.Scale, EulerRotations, Transform.Translation, IsInsideOut, Corners.ToList(), newID);
		}

		public override string ToString() {
			return "Plane || " + ShapeDesc.Normal + " normal, " + Corners.Count + " corners" + GetTransformString();
		}

		public override string GetShortDesc() {
			return "Plane [" + Corners.Count + "c]";
		}

		public override PhysicsShapeHandle CreatePhysicsShape(out Vector3 shapeOffset) {
			shapeOffset = Vector3.ZERO;
			return PhysicsShapeHandle.NULL;
		}
	}

	public sealed class LevelGeometry_Model : LevelGeometry {
		private const string FILE_NAME_DESC_ATTRIBUTE_NAME = "filename";
		private const string MIRROR_X_ATTRIBUTE_NAME = "mirrorX";
		public readonly string ModelFileName;
		public readonly bool MirrorX;

		public LevelGeometry_Model(string modelFileName, bool mirrorX, int id)
			: base(Vector2.ONE, Vector3.ONE, Vector3.ZERO, Vector3.ZERO, false, id) {
			ModelFileName = modelFileName;
			MirrorX = mirrorX;
		}

		public LevelGeometry_Model(string modelFileName, bool mirrorX, Vector3 scale, int id)
			: base(Vector2.ONE, scale, Vector3.ZERO, Vector3.ZERO, false, id) {
			ModelFileName = modelFileName;
			MirrorX = mirrorX;
		}

		public LevelGeometry_Model(XElement xmlElement) : base(xmlElement) {
			ModelFileName = xmlElement.Attribute(FILE_NAME_DESC_ATTRIBUTE_NAME).Value;
			MirrorX = bool.Parse(xmlElement.Attribute(MIRROR_X_ATTRIBUTE_NAME).Value);
		}

		public override void GetVertexData(out List<DefaultVertex> outVertices, out List<uint> outIndices) {
			var modelData = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, ModelFileName));
			outIndices = modelData.GetIndices().ToList();
			outVertices = modelData.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) })).ToList();
			if (MirrorX) {
				for (int i = 0; i < outVertices.Count; i++) {
					Vector3 originalPos = outVertices[i].Position.Scale(Transform.Scale);
					outVertices[i] = new DefaultVertex(
						new Vector3(originalPos, x: -originalPos.X),
						new Vector3(outVertices[i].Normal, x: -outVertices[i].Normal.X),
						outVertices[i].TexUV
					);
				}
				for (int i = 0; i < outIndices.Count; i += 3) {
					uint index1 = outIndices[i];
					outIndices[i] = outIndices[i + 1];
					outIndices[i + 1] = index1;
				}
			}
			else {
				for (int i = 0; i < outVertices.Count; i++) {
					outVertices[i] = new DefaultVertex(
						outVertices[i].Position.Scale(Transform.Scale),
						outVertices[i].Normal,
						outVertices[i].TexUV
					);
				}
			}
		}

		private void GetVertexDataUnscaled(out List<DefaultVertex> outVertices, out List<uint> outIndices) {
			var modelData = AssetLoader.LoadModel(Path.Combine(AssetLocator.ModelsDir, ModelFileName));
			outIndices = modelData.GetIndices().ToList();
			outVertices = modelData.GetVertices<DefaultVertex>(typeof(DefaultVertex).GetConstructor(new[] { typeof(Vector3), typeof(Vector3), typeof(Vector3), typeof(Vector2) })).ToList();
			if (MirrorX) {
				for (int i = 0; i < outVertices.Count; i++) {
					Vector3 originalPos = outVertices[i].Position;
					outVertices[i] = new DefaultVertex(
						new Vector3(originalPos, x: -originalPos.X),
						new Vector3(outVertices[i].Normal, x: -outVertices[i].Normal.X),
						outVertices[i].TexUV
					);
				}
				for (int i = 0; i < outIndices.Count; i += 3) {
					uint index1 = outIndices[i];
					outIndices[i] = outIndices[i + 1];
					outIndices[i + 1] = index1;
				}
			}
			else {
				for (int i = 0; i < outVertices.Count; i++) {
					outVertices[i] = new DefaultVertex(
						outVertices[i].Position,
						outVertices[i].Normal,
						outVertices[i].TexUV
					);
				}
			}
		}

		public override XElement Serialize() {
			XElement result = new XElement(MODEL_ELEMENT_NAME);
			result.SetAttributeValue(FILE_NAME_DESC_ATTRIBUTE_NAME, ModelFileName);
			result.SetAttributeValue(MIRROR_X_ATTRIBUTE_NAME, MirrorX);
			AddGenericSerializationProperties(result);
			return result;
		}

		public override LevelGeometry Clone(int newID) {
			return new LevelGeometry_Model(ModelFileName, MirrorX, newID);
		}

		public override string ToString() {
			return "Model || \"" + ModelFileName + "\"" + (MirrorX ? " (mirrored)" : String.Empty) + GetTransformString();
		}

		public override string GetShortDesc() {
			return "Model [" + ModelFileName + (MirrorX ? "<>" : String.Empty) + "]";
		}

		public override PhysicsShapeHandle CreatePhysicsShape(out Vector3 shapeOffset) {
			shapeOffset = Vector3.ZERO;

			if (this.Transform == Transform.DEFAULT_TRANSFORM) {
				var potentialResult = WorldModelCache.GetCachedModel(ModelFileName);
				if (potentialResult.HasValue) return potentialResult.Value;
			}

			List<DefaultVertex> outVerts;
			List<uint> outIndices;
			GetVertexDataUnscaled(out outVerts, out outIndices);

			string acdFileName = ModelFileName;
			if (MirrorX) acdFileName = Path.GetFileNameWithoutExtension(acdFileName) + "_mirrored" + Path.GetExtension(acdFileName);
			var result = PhysicsManager.CreateConcaveHullShape(
				outVerts.Select(df => df.Position), 
				outIndices.Select(@uint => (int) @uint), 
				new CollisionShapeOptionsDesc(Transform.Scale),
				AssetLocator.CreateACDFilePath(acdFileName)
			);

			if (this.Transform == Transform.DEFAULT_TRANSFORM) WorldModelCache.SetCachedModel(ModelFileName, result);

			return result;
		}

		public bool HasIdenticalPhysicsShape(LevelGeometry_Model other) {
			return other.ModelFileName == ModelFileName && other.Transform.Scale == Transform.Scale;
		}
	}
}