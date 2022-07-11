struct raymarch_options {
	float minDistance;
	float maxDistance;
	int minDepth;
	int maxDepth;
	float scale;
	float stretch;
	float step;
	float stepDelta;
	float distanceFactor;
	float error;
};

//https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
float sign(float2 p1, float2 p2, float2 p3)
{
	return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
}

//https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
bool pointInTriangle(float2 pt, float2 v1, float2 v2, float2 v3)
{
	float d1, d2, d3;
	bool has_neg, has_pos;

	d1 = sign(pt, v1, v2);
	d2 = sign(pt, v2, v3);
	d3 = sign(pt, v3, v1);

	has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
	has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

	return !(has_neg && has_pos);
}

// PI = 3.14159265359f
float rand(float seed0, float seed1) {
	return frac((seed0 + 3.14159265359f) * (seed1 + 3.14159265359f)) * 2.0f - 1.0f;
}

int getTargetDepth(raymarch_options o, float distance) {
	distance -= o.minDistance / o.stretch;
	float maxDistance = o.maxDistance - o.minDistance / o.stretch;

	float max = (o.maxDistance / o.stretch) * (o.maxDistance / o.stretch);
	float min = (o.minDistance / o.stretch) * (o.minDistance / o.stretch);
	float t = (distance / o.stretch) * (distance / o.stretch);

	float mix = lerp((float)o.maxDepth, (float)o.minDepth, (t - min) / (max - min));
	return (int)mix;
}

float rightTriangle(float2 p, int depth, float distanceFactor) {
	float d = distanceFactor;

	// x and y are coordinates. z is height. w is random number
	float4 a = float4(0.0f, 1.0f, 0.0f, 0.4321558372f);
	float4 b = float4(0.0f, 0.0f, 0.0f, -0.9770926823f);
	float4 c = float4(1.0f, 0.0f, 0.0f, 0.2236396734f);

	int initialDepth = 0;

	if (!pointInTriangle(p, a.xy, b.xy, c.xy)) {
		return -2;
	}

	for (int i = initialDepth; i < depth; i++)
	{
		float r = rand(a.w, c.w);
		float h = (a.z + c.z) / 2 + r * length(a.xy - c.xy) * d;
		float4 ac = float4((a.xy + c.xy) / 2.0f, h, r);

		float distA = length(p - a.xy);
		float distC = length(p - c.xy);
		if (distA < distC)
		{
			c = b;
			b = ac;
		}
		else if (distA > distC)
		{
			a = c;
			c = b;
			b = ac;
		}
		else if (a.r > c.r)
		{
			c = b;
			b = ac;
		}
		else
		{
			a = c;
			c = b;
			b = ac;
		}
	}

	return (a.z + b.z + c.z) / 3.0f;
}

float flat(float2 p) {
	return 0;
}

//https://iquilezles.org/articles/terrainmarching/
float castRay(raymarch_options o, float3 origin, float3 surface)
{
	float3 direction = normalize(surface - origin);
	float surfaceDistance = length(surface - origin);

	float dt = o.step;
	const float mint = max(surfaceDistance, o.minDistance);
	const float maxt = o.maxDistance;

	float3 lastPoint = surface;

	float lh = 0.0f;
	float ly = 0.0f;

	for (float t = mint; t < maxt; t += dt)
	{
		const float3 p = origin + direction * t;

		if (p.y > o.scale) {
			return false;
		}

		int targetDepth = getTargetDepth(o, t);

		float h = rightTriangle(p.xz / o.stretch, targetDepth, o.distanceFactor) * o.scale;

		if (p.y < h)
		{
			return (t - dt + dt * (lh - ly) / (p.y - ly - h + lh));
		}
		else if (p.y - h > o.error) {
			return -1;
		}

		lh = h;
		ly = p.y;

		lastPoint = p;
		dt = dt + o.stepDelta;
	}
	return -1;
}

//https://stackoverflow.com/questions/49640250/calculate-normals-from-heightmap
//https://www.ultraengine.com/community/topic/16244-calculate-normals-from-heightmap/
float3 getNormal(raymarch_options o, float3 p, float distance) {
	int targetDepth = getTargetDepth(o, distance);

	float n = sqrt(2.0f) * o.stretch;
	for (int i = 0; i < targetDepth; i++)
	{
		n = sqrt((n / 2.0f) * (n / 2.0f));
	}

	float diff = max(n * 2.0f, 10.0f);

	float2 Rxz = float2(p.x + diff, p.z);
	float2 Lxz = float2(p.x - diff, p.z);
	float2 Txz = float2(p.x, p.z + diff);
	float2 Bxz = float2(p.x, p.z - diff);

	float R = rightTriangle(Rxz / o.stretch, targetDepth, o.distanceFactor) * o.scale;
	float L = rightTriangle(Lxz / o.stretch, targetDepth, o.distanceFactor) * o.scale;
	float T = rightTriangle(Txz / o.stretch, targetDepth, o.distanceFactor) * o.scale;
	float B = rightTriangle(Bxz / o.stretch, targetDepth, o.distanceFactor) * o.scale;

	float3 retval = float3(2.0f * diff * (L - R), 4.0f * diff * diff, 2.0f * diff * (B - T));

	return normalize(retval);
}

//https://forums.ogre3d.org/viewtopic.php?t=74489
float3 worldToTangentNormalVector(float3 wNormal, float3 wTangent, float3 wTerrainNormal) {
	float3 triangleNormal = normalize(wNormal);
	float3 triangleTangent = normalize(wTangent);
	float3 triangleBinormal = normalize(cross(wNormal, wTangent));
	triangleTangent = cross(triangleBinormal, triangleNormal);// fix tangent after interpolation

	float3 tangentSpaceNormal = 0;
	tangentSpaceNormal.x = dot(wTerrainNormal, triangleTangent);
	tangentSpaceNormal.y = dot(wTerrainNormal, triangleBinormal);
	tangentSpaceNormal.z = dot(wTerrainNormal, triangleNormal);
	return tangentSpaceNormal;
}