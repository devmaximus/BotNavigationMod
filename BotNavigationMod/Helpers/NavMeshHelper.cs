using UnityEngine;
using UnityEngine.AI;

namespace BotNavigationMod.Helpers;

public sealed class NavMeshHelper
{
    private readonly NavMeshPath _reusablePath = new NavMeshPath();

    public bool TryGetReachablePosition(
        Vector3 from,
        Vector3 candidate,
        float sampleRadius,
        float maxYDelta,
        out Vector3 result)
    {
        result = candidate;

        if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
        {
            return false;
        }

        if (Mathf.Abs(hit.position.y - candidate.y) > maxYDelta)
        {
            return false;
        }

        if (!NavMesh.CalculatePath(from, hit.position, NavMesh.AllAreas, _reusablePath))
        {
            return false;
        }

        if (_reusablePath.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        result = hit.position;
        return true;
    }
}
