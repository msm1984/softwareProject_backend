using System.Security.Claims;
using AnalysisData.Dtos.GraphDto.EdgeDto;
using AnalysisData.Dtos.GraphDto.NodeDto;
using AnalysisData.Exception.GraphException.EdgeException;
using AnalysisData.Exception.GraphException.NodeException;
using AnalysisData.Repositories.GraphRepositories.GraphRepository.GraphEdgeRepository.Abstraction;
using AnalysisData.Repositories.GraphRepositories.GraphRepository.GraphNodeRepository.Abstraction;
using AnalysisData.Services.GraphService.GraphServices.NodeAndEdgeInfo.Abstraction;

namespace AnalysisData.Services.GraphService.GraphServices.NodeAndEdgeInfo;

public class NodeAndEdgeInfo : INodeAndEdgeInfo
{
    private readonly IGraphNodeRepository _graphNodeRepository;
    private readonly IGraphEdgeRepository _graphEdgeRepository;

    public NodeAndEdgeInfo(IGraphNodeRepository graphNodeRepository, IGraphEdgeRepository graphEdgeRepository)
    {
        _graphNodeRepository = graphNodeRepository;
        _graphEdgeRepository = graphEdgeRepository;
    }

    public async Task<Dictionary<string, string>> GetNodeInformationAsync(ClaimsPrincipal claimsPrincipal,
        Guid nodeId)
    {
        var role = claimsPrincipal.FindFirstValue(ClaimTypes.Role);
        var username = claimsPrincipal.FindFirstValue("id");
        var result = Enumerable.Empty<NodeInformationDto>();
        var usernameGuid = Guid.Parse(username);
        if (role != "data-analyst")
        {
            result = await _graphNodeRepository.GetNodeAttributeValueAsync(nodeId);
        }
        else if (await _graphNodeRepository.IsNodeAccessibleByUser(usernameGuid, nodeId))
        {
            result = await _graphNodeRepository.GetNodeAttributeValueAsync(nodeId);
        }

        if (!result.Any())
        {
            throw new NodeNotFoundException();
        }

        var output = new Dictionary<string, string>();
        foreach (var item in result)
        {
            output[item.Attribute] = item.Value;
        }

        return output;
    }

    public async Task<Dictionary<string, string>> GetEdgeInformationAsync(ClaimsPrincipal claimsPrincipal, Guid edgeId)
    {
        var role = claimsPrincipal.FindFirstValue(ClaimTypes.Role);
        var username = claimsPrincipal.FindFirstValue("id");
        var result = Enumerable.Empty<EdgeInformationDto>();
        if (role != "data-analyst")
        {
            result = await _graphEdgeRepository.GetEdgeAttributeValues(edgeId);
        }
        else if (await _graphEdgeRepository.IsEdgeAccessibleByUser(username, edgeId))
        {
            result = await _graphEdgeRepository.GetEdgeAttributeValues(edgeId);
        }

        if (result.Count() == 0)
        {
            throw new EdgeNotFoundException();
        }

        var output = new Dictionary<string, string>();

        foreach (var item in result)
        {
            output[item.Attribute] = item.Value;
        }

        return output;
    }
}