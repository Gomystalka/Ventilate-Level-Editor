// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
//Based on the SpatialMapping/WireFrame shader.
//Modified by Tomasz Galka. 
// -Added _Color, _WireColor, _MainTex, _TexScale, _EdgeDetectionThreshold and _MaxThickness. Removed Spatial Mapping functionality. Acts as a wireframe shader.
Shader "Tom/Wireframe"
{
	Properties
	{
		_WireThickness("Wire Thickness", Float) = 100
		_MaxThickness ("Max Wire Thickness", Float) = 800
		[HDR] _Color ("Base Color", Color) = (1, 1, 1, 1)
		[HDR] _WireColor("Wire Color", Color) = (1, 1, 1, 1)
		_MainTex("Texture", 2D) = "white" {}
		_TexScale("Texture Scale", Float) = 1.0
		_EdgeDetectionThreshold("Edge Threshold", Float) = 0.9
		[Toggle(USE_VERTEX_COLORS)] _UseVertexColors("Use Vertex Colors", Float) = 0.0
	}

		SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent"}

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
		// Wireframe shader based on the the following
		// http://developer.download.nvidia.com/SDK/10/direct3d/Source/SolidWireframe/Doc/SolidWireframe.pdf
		CGPROGRAM
		#pragma vertex vert
		#pragma geometry geom
		#pragma fragment frag
		#pragma shader_feature USE_VERTEX_COLORS

		#include "UnityCG.cginc"

		float _WireThickness;
		float _MaxThickness;
		float4 _Color;
		float4 _WireColor;
		sampler2D _MainTex;
		float4 _MainTex_ST;
		float _EdgeDetectionThreshold;
		float _TexScale;

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			float4 color : COLOR;
		};

		struct v2g
		{
			float4 projectionSpaceVertex : SV_POSITION;
			float2 uv : TEXCOORD0;
			float4 worldSpacePosition : TEXCOORD1;
			fixed4 vertexColor : COLOR;
		};

		struct g2f
		{
			float4 projectionSpaceVertex : SV_POSITION;
			float2 uv : TEXCOORD0;
			float4 worldSpacePosition : TEXCOORD1;
			float4 dist : TEXCOORD2;
			float4 color : COLOR;
		};

		v2g vert(appdata v)
		{
			v2g o;
			UNITY_SETUP_INSTANCE_ID(v);
			o.projectionSpaceVertex = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			o.vertexColor = v.color;
			o.worldSpacePosition = mul(unity_ObjectToWorld, v.vertex);
			return o;
		}

		[maxvertexcount(3)]
		void geom(triangle v2g i[3], inout TriangleStream<g2f> triangleStream)
		{
			float2 p0 = i[0].projectionSpaceVertex.xy / i[0].projectionSpaceVertex.w;
			float2 p1 = i[1].projectionSpaceVertex.xy / i[1].projectionSpaceVertex.w;
			float2 p2 = i[2].projectionSpaceVertex.xy / i[2].projectionSpaceVertex.w;

			float2 edge0 = p2 - p1;
			float2 edge1 = p2 - p0;
			float2 edge2 = p1 - p0;

			// To find the distance to the opposite edge, we take the
			// formula for finding the area of a triangle Area = Base/2 * Height,
			// and solve for the Height = (Area * 2)/Base.
			// We can get the area of a triangle by taking its cross product
			// divided by 2.  However we can avoid dividing our area/base by 2
			// since our cross product will already be double our area.
			float area = abs(edge1.x * edge2.y - edge1.y * edge2.x);
			float wireThickness = _MaxThickness - _WireThickness;

			g2f o;
			o.worldSpacePosition = i[0].worldSpacePosition;
			o.projectionSpaceVertex = i[0].projectionSpaceVertex;
			o.dist.xyz = float3((area / length(edge0)), 0.0, 0.0) * o.projectionSpaceVertex.w * wireThickness;
			o.dist.w = 1.0 / o.projectionSpaceVertex.w;
			o.uv = i[0].uv;
			o.color = i[0].vertexColor;
			triangleStream.Append(o);

			o.worldSpacePosition = i[1].worldSpacePosition;
			o.projectionSpaceVertex = i[1].projectionSpaceVertex;
			o.dist.xyz = float3(0.0, (area / length(edge1)), 0.0) * o.projectionSpaceVertex.w * wireThickness;
			o.dist.w = 1.0 / o.projectionSpaceVertex.w;
			o.uv = i[1].uv;
			o.color = i[1].vertexColor;
			triangleStream.Append(o);

			o.worldSpacePosition = i[2].worldSpacePosition;
			o.projectionSpaceVertex = i[2].projectionSpaceVertex;
			o.dist.xyz = float3(0.0, 0.0, (area / length(edge2))) * o.projectionSpaceVertex.w * wireThickness;
			o.dist.w = 1.0 / o.projectionSpaceVertex.w;
			o.uv = i[2].uv;
			o.color = i[2].vertexColor;
			triangleStream.Append(o);
		}

		fixed4 frag(g2f i) : SV_Target
		{
			fixed4 col = tex2D(_MainTex, i.uv * _TexScale);
			float minDistanceToEdge = min(i.dist[0], min(i.dist[1], i.dist[2])) * i.dist[3];

		// Early out if we know we are not on a line segment.
			if (minDistanceToEdge > _EdgeDetectionThreshold)
#ifdef USE_VERTEX_COLORS
				return col * i.color;
#else
				return col * _Color;
#endif
		// Smooth our line out
		float t = exp2(-2 * minDistanceToEdge * minDistanceToEdge);
		fixed4 wireColor = _WireColor;

		fixed4 finalColor = lerp(float4(0,0,0,1), wireColor, t);
		finalColor.a = t;

		return finalColor;
	}
	ENDCG
}
	}
}