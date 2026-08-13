using System;
using System.Collections.Generic;
using System.Linq;
using StudyHub.Application.DTOs.Dashboard;
using StudyHub.Infrastructure.Services.Ai;
using Xunit;
using Xunit.Abstractions;

namespace StudyHub.Tests
{
    public class Full42AiRegressionTests
    {
        private readonly ITestOutputHelper _output;
        private readonly AiIntentDetector _detector;
        private readonly AiContextBuilder _contextBuilder;
        private readonly AiPromptBuilder _promptBuilder;

        public Full42AiRegressionTests(ITestOutputHelper output)
        {
            _output = output;
            _detector = new AiIntentDetector();
            _contextBuilder = new AiContextBuilder();
            _promptBuilder = new AiPromptBuilder();
        }

        [Theory]
        // Group 1: TASK_PRIORITIZATION
        [InlineData("Hôm nay tôi nên làm task nào trước?", AiIntents.TaskPrioritization)]
        [InlineData("Task nào cần ưu tiên trước?", AiIntents.TaskPrioritization)]
        [InlineData("Tôi nên làm task nào trước?", AiIntents.TaskPrioritization)]
        [InlineData("Việc nào cần làm ngay?", AiIntents.TaskPrioritization)]
        [InlineData("Task nào quan trọng nhất?", AiIntents.TaskPrioritization)]
        [InlineData("Task nào gấp nhất?", AiIntents.TaskPrioritization)]
        [InlineData("Trong 3 ngày tới tôi nên ưu tiên task nào?", AiIntents.TaskPrioritization)]
        [InlineData("Tôi có 3 task quá hạn, nên làm task nào trước?", AiIntents.TaskPrioritization)]
        [InlineData("Hôm nay tôi nên làm gì?", AiIntents.TaskPrioritization)]
        // Group 2: SCHEDULE_QUERY
        [InlineData("Hôm nay có lịch gì?", AiIntents.ScheduleQuery)]
        [InlineData("Hôm nay tôi có lịch học gì?", AiIntents.ScheduleQuery)]
        [InlineData("Ngày mai tôi có lịch thi gì?", AiIntents.ScheduleQuery)]
        [InlineData("Tuần này tôi có lịch học nào?", AiIntents.ScheduleQuery)]
        [InlineData("Tuần này tôi có lịch thi nào?", AiIntents.ScheduleQuery)]
        [InlineData("Tôi có lịch thi môn Cơ sở dữ liệu khi nào?", AiIntents.ScheduleQuery)]
        [InlineData("Hôm nay tôi có lịch học hay lịch thi không?", AiIntents.ScheduleQuery)]
        // Group 3: KNOWLEDGE_QA
        [InlineData("Java interface là gì?", AiIntents.KnowledgeQa)]
        [InlineData("SQL JOIN là gì?", AiIntents.KnowledgeQa)]
        [InlineData("Dijkstra là gì?", AiIntents.KnowledgeQa)]
        [InlineData("Ma trận Eisenhower là gì?", AiIntents.KnowledgeQa)]
        [InlineData("Deadline và priority khác nhau thế nào?", AiIntents.KnowledgeQa)]
        [InlineData("Deadline hay độ ưu tiên quan trọng hơn?", AiIntents.KnowledgeQa)]
        [InlineData("Task quá hạn có phải luôn ưu tiên cao nhất không?", AiIntents.KnowledgeQa)]
        [InlineData("Task nào tôi nên làm trước nếu có một task hạn hôm nay nhưng ưu tiên thấp và một task hạn tuần sau nhưng ưu tiên cao?", AiIntents.KnowledgeQa)]
        // Group 4: GENERAL_CHAT
        [InlineData("Xin chào", AiIntents.GeneralChat)]
        [InlineData("Hôm nay thế nào?", AiIntents.GeneralChat)]
        [InlineData("Cảm ơn nhé", AiIntents.GeneralChat)]
        [InlineData("Bạn khỏe không?", AiIntents.GeneralChat)]
        [InlineData("Tôi mệt quá", AiIntents.GeneralChat)]
        [InlineData("Chán quá", AiIntents.GeneralChat)]
        [InlineData("Giúp tôi với", AiIntents.GeneralChat)]
        [InlineData("Haha", AiIntents.GeneralChat)]
        [InlineData("Ok", AiIntents.GeneralChat)]
        [InlineData("Được rồi", AiIntents.GeneralChat)]
        // Group 5: TASK_QUERY
        [InlineData("Tôi có task nào quá hạn không?", AiIntents.TaskQuery)]
        [InlineData("Task nào đang quá hạn?", AiIntents.TaskQuery)]
        [InlineData("Hôm nay tôi có task nào cần làm không?", AiIntents.TaskQuery)]
        [InlineData("Tôi còn những việc nào chưa hoàn thành hôm nay?", AiIntents.TaskQuery)]
        // Group 6: WORKLOAD_ANALYSIS
        [InlineData("Tôi có đang quá tải không?", AiIntents.WorkloadAnalysis)]
        [InlineData("Khối lượng công việc của tôi hiện tại thế nào?", AiIntents.WorkloadAnalysis)]
        [InlineData("Tôi có quá nhiều task không?", AiIntents.WorkloadAnalysis)]
        [InlineData("Tôi đang có bao nhiêu công việc cần hoàn thành?", AiIntents.WorkloadAnalysis)]
        public void Test_All42_IntentDetections_ShouldMatchExpected(string userQuery, string expectedIntent)
        {
            var result = _detector.DetectIntent(userQuery);
            _output.WriteLine($"Query: \"{userQuery}\" => Detected: {result.Intent} (Score: {result.Score}) | Expected: {expectedIntent}");
            Assert.Equal(expectedIntent, result.Intent);
        }
    }
}
