using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HeroPrism.Api.Infrastructure;
using MediatR;
using Nerdino.Controllerless;
using StreamChat;

namespace HeroPrism.Api.Features.Chat
{
    [ApiRequest("chat", "", ActionType.Read, true)]
    public class GetChatRoomsRequest : IRequest<GetChatRoomsResponse>
    {
    }

    public class GetChatRoomsRequestHandler : IRequestHandler<GetChatRoomsRequest, GetChatRoomsResponse>
    {
        private readonly IClient _chatClient;

        public GetChatRoomsRequestHandler(IClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<GetChatRoomsResponse> Handle(GetChatRoomsRequest request, CancellationToken cancellationToken)
        {
            var channels = await _chatClient.QueryChannels(new QueryChannelsOptions());

            var response = new GetChatRoomsResponse {ChatRooms = new List<ChatRoomResponse>()};
            foreach (var channel in channels)
            {
                response.ChatRooms.Add(new ChatRoomResponse()
                    {Id = channel.Channel.ID, Users = channel.Members.Select(c => c.User.ID)});
            }

            return response;
        }
    }

    public class GetChatRoomsResponse
    {
        public List<ChatRoomResponse> ChatRooms { get; set; }
    }

    public class ChatRoomResponse
    {
        public string Id { get; set; }
        public IEnumerable<string> Users { get; set; }
    }
}