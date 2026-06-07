using NutriScan.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NutriScan.Services
{
    public static class DatabaseSeeder
    {
        public static void Seed(NutriScanDbContext context)
        {
            // Seed default UserProfile if none exists
            if (!context.UserProfiles.Any())
            {
                context.UserProfiles.Add(new UserProfile
                {
                    Name = "Nguyễn Văn A",
                    Age = 25,
                    Gender = "Nam",
                    Height = 170,
                    CurrentWeight = 70,
                    TargetWeight = 65,
                    ActivityLevel = "Moderate", // Sedentary, Light, Moderate, Active, VeryActive
                    GoalType = "Lose", // Lose, Maintain, Gain
                    DailyCalorieTarget = 1850,
                    DailyFatTarget = 55,
                    DailyCarbTarget = 220,
                    DailyProteinTarget = 120,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                });
                context.SaveChanges();
            }

            // Seed WorkoutPlans if none exists
            if (!context.WorkoutPlans.Any())
            {
                var workouts = new List<WorkoutPlan>
                {
                    // CARDIO
                    new WorkoutPlan {
                        Name = "Chạy bộ chậm (Joggling)",
                        Type = "Cardio",
                        DurationMinutes = 30,
                        CaloriesBurned = 240,
                        Difficulty = "Dễ",
                        MuscleGroup = "Toàn thân",
                        Description = "Chạy bộ với tốc độ vừa phải giúp duy trì nhịp tim ổn định và đốt cháy mỡ thừa hiệu quả.",
                        ImageUrl = "fa-running",
                        Instructions = "Khởi động khớp cổ tay chân 5 phút;Chạy bước nhỏ tại chỗ;Bắt đầu chạy tốc độ chậm, hít thở sâu;Giảm dần tốc độ và đi bộ thả lỏng 3 phút."
                    },
                    new WorkoutPlan {
                        Name = "Chạy bộ cường độ cao (HIIT)",
                        Type = "Cardio",
                        DurationMinutes = 20,
                        CaloriesBurned = 300,
                        Difficulty = "Khó",
                        MuscleGroup = "Toàn thân",
                        Description = "Chạy nhanh kết hợp nghỉ ngắn xen kẽ giúp tối ưu hóa lượng calo đốt cháy trong thời gian ngắn.",
                        ImageUrl = "fa-bolt",
                        Instructions = "Khởi động kỹ trong 5 phút;Chạy nước rút hết sức 30 giây;Đi bộ chậm phục hồi 60 giây;Lặp lại vòng tuần hoàn này 10-12 lần."
                    },
                    new WorkoutPlan {
                        Name = "Nhảy dây (Jump Rope)",
                        Type = "Cardio",
                        DurationMinutes = 15,
                        CaloriesBurned = 180,
                        Difficulty = "Trung bình",
                        MuscleGroup = "Mông/Đùi",
                        Description = "Bài tập đơn giản nhưng cực kỳ hiệu quả để rèn luyện sức bền và sự linh hoạt của cổ chân.",
                        ImageUrl = "fa-bounce",
                        Instructions = "Chọn dây nhảy có độ dài phù hợp;Nhảy bằng 2 nửa bàn chân trước;Giữ khuỷu tay gần sát thân người;Duy trì nhịp nhảy đều đặn."
                    },
                    new WorkoutPlan {
                        Name = "Bài tập leo núi tại chỗ (Mountain Climbers)",
                        Type = "Cardio",
                        DurationMinutes = 10,
                        CaloriesBurned = 100,
                        Difficulty = "Trung bình",
                        MuscleGroup = "Bụng",
                        Description = "Kết hợp tư thế plank và chạy tại chỗ để đốt mỡ bụng hiệu quả.",
                        ImageUrl = "fa-mountain",
                        Instructions = "Bắt đầu ở tư thế chống đẩy plank;Kéo đầu gối phải về phía ngực;Đổi chân nhanh chóng sang đầu gối trái;Giữ lưng thẳng, không để mông nhô quá cao."
                    },
                    new WorkoutPlan {
                        Name = "Nhảy Burpees",
                        Type = "Cardio",
                        DurationMinutes = 12,
                        CaloriesBurned = 150,
                        Difficulty = "Khó",
                        MuscleGroup = "Toàn thân",
                        Description = "Bài tập phối hợp toàn thân giúp kích thích nhịp tim tăng nhanh và đốt cháy calo siêu tốc.",
                        ImageUrl = "fa-fire",
                        Instructions = "Đứng thẳng rồi squat xuống đất đặt hai tay trước mặt;Nhảy lùi hai chân về tư thế chống đẩy;Hít đất một nhịp rồi nhảy thu chân lại;Bật nhảy lên cao, hai tay giơ qua đầu."
                    },
                    new WorkoutPlan {
                        Name = "Nhảy Jumping Jacks",
                        Type = "Cardio",
                        DurationMinutes = 15,
                        CaloriesBurned = 120,
                        Difficulty = "Dễ",
                        MuscleGroup = "Toàn thân",
                        Description = "Bài tập khởi động và làm nóng cơ thể lý tưởng, kích hoạt tuần hoàn máu.",
                        ImageUrl = "fa-child-reaching",
                        Instructions = "Đứng thẳng, hai chân khép lại, tay đặt sát bên hông;Bật nhảy rộng chân ra đồng thời vung hai tay lên trên đầu;Bật nhảy trở lại tư thế ban đầu;Lặp lại liên tục."
                    },
                    new WorkoutPlan {
                        Name = "Đạp xe trong nhà (Cycling)",
                        Type = "Cardio",
                        DurationMinutes = 30,
                        CaloriesBurned = 220,
                        Difficulty = "Dễ",
                        MuscleGroup = "Mông/Đùi",
                        Description = "Tốt cho tim mạch và xương khớp, giảm áp lực lên các khớp gối.",
                        ImageUrl = "fa-bicycle",
                        Instructions = "Điều chỉnh yên xe cao ngang hông;Đạp xe tốc độ trung bình khởi động 5 phút;Tăng kháng lực đạp nhanh trong 20 phút;Đạp chậm dần để làm nguội cơ thể."
                    },
                    new WorkoutPlan {
                        Name = "Bài tập đấm bốc bóng (Shadow Boxing)",
                        Type = "Cardio",
                        DurationMinutes = 20,
                        CaloriesBurned = 160,
                        Difficulty = "Dễ",
                        MuscleGroup = "Lưng/Tay",
                        Description = "Rèn luyện phản xạ nhanh nhạy và săn chắc cơ vai, bắp tay.",
                        ImageUrl = "fa-hand-fist",
                        Instructions = "Đứng ở tư thế phòng thủ boxing;Thực hiện các cú đấm thẳng (jab, cross);Kết hợp đấm móc và né đòn;Di chuyển chân liên tục."
                    },

                    // STRENGTH
                    new WorkoutPlan {
                        Name = "Hít đất cơ bản (Push-ups)",
                        Type = "Strength",
                        DurationMinutes = 15,
                        CaloriesBurned = 110,
                        Difficulty = "Trung bình",
                        MuscleGroup = "Ngực/Vai",
                        Description = "Bài tập bodyweight kinh điển để phát triển cơ ngực, vai và bắp tay sau.",
                        ImageUrl = "fa-arrows-down-to-line",
                        Instructions = "Chống hai tay rộng hơn vai một chút;Giữ cơ thể thẳng từ đầu đến gót chân;Hạ ngực xuống sát sàn, hít vào;Đẩy người lên trở lại, thở ra."
                    },
                    new WorkoutPlan {
                        Name = "Squats cơ bản (Air Squat)",
                        Type = "Strength",
                        DurationMinutes = 15,
                        CaloriesBurned = 100,
                        Difficulty = "Dễ",
                        MuscleGroup = "Mông/Đùi",
                        Description = "Tăng cường sức mạnh phần thân dưới, đùi trước và mông săn chắc.",
                        ImageUrl = "fa-angles-down",
                        Instructions = "Đứng chân rộng bằng vai, mũi chân hơi hướng ra ngoài;Hạ hông xuống như đang ngồi vào ghế;Giữ đầu gối không vượt quá mũi chân;Ấn gót chân đẩy người đứng dậy thẳng lưng."
                    },
                    new WorkoutPlan {
                        Name = "Tư thế tấm ván (Plank)",
                        Type = "Strength",
                        DurationMinutes = 10,
                        CaloriesBurned = 60,
                        Difficulty = "Dễ",
                        MuscleGroup = "Bụng",
                        Description = "Xây dựng cơ bụng và cơ lõi khỏe mạnh, cải thiện tư thế đứng thẳng lưng.",
                        ImageUrl = "fa-grip-lines",
                        Instructions = "Chống khuỷu tay vuông góc dưới vai;Chân duỗi thẳng, tì mũi chân xuống đất;Siết chặt cơ bụng và mông;Giữ tư thế thở đều trong 30-60 giây."
                    },
                    new WorkoutPlan {
                        Name = "Bước chùng chân (Lunges)",
                        Type = "Strength",
                        DurationMinutes = 15,
                        CaloriesBurned = 95,
                        Difficulty = "Dễ",
                        MuscleGroup = "Mông/Đùi",
                        Description = "Bài tập tuyệt vời cải thiện thăng bằng và sức mạnh đơn khớp chân.",
                        ImageUrl = "fa-shoe-prints",
                        Instructions = "Đứng thẳng, bước một chân rộng lên phía trước;Hạ thấp cơ thể sao cho hai đầu gối tạo góc 90 độ;Đầu gối sau gần chạm sàn;Rút chân về vị trí ban đầu và đổi chân."
                    },
                    new WorkoutPlan {
                        Name = "Gập bụng (Crunches)",
                        Type = "Strength",
                        DurationMinutes = 15,
                        CaloriesBurned = 80,
                        Difficulty = "Dễ",
                        MuscleGroup = "Bụng",
                        Description = "Tập trung lực vào nhóm cơ bụng trên giúp múi bụng nổi rõ.",
                        ImageUrl = "fa-square-check",
                        Instructions = "Nằm ngửa, gối gập, hai bàn chân đặt trên sàn;Đặt nhẹ tay sau gáy (không kéo cổ);Dùng cơ bụng nâng vai lên khỏi mặt đất;Hạ xuống chậm rãi."
                    },
                    new WorkoutPlan {
                        Name = "Tập cơ tay sau với ghế (Tricep Dips)",
                        Type = "Strength",
                        DurationMinutes = 12,
                        CaloriesBurned = 90,
                        Difficulty = "Trung bình",
                        MuscleGroup = "Lưng/Tay",
                        Description = "Loại bỏ mỡ thừa cánh tay sau, giúp tay săn chắc và khỏe mạnh.",
                        ImageUrl = "fa-chair",
                        Instructions = "Đặt tay lên mép ghế chắc chắn phía sau;Đưa mông ra khỏi ghế, chân hơi gập hoặc duỗi thẳng;Gập khuỷu tay hạ thấp người xuống góc vai 90 độ;Đẩy người lên bằng lực cơ tay sau."
                    },
                    new WorkoutPlan {
                        Name = "Nằm nâng hông (Glute Bridges)",
                        Type = "Strength",
                        DurationMinutes = 15,
                        CaloriesBurned = 85,
                        Difficulty = "Dễ",
                        MuscleGroup = "Mông/Đùi",
                        Description = "Kích hoạt cơ mông và giảm áp lực nhức mỏi vùng thắt lưng.",
                        ImageUrl = "fa-bridge",
                        Instructions = "Nằm ngửa, co gối, bàn chân đặt song song trên thảm;Ấn gót chân, đẩy hông lên cao thẳng hàng đùi và lưng;Siết chặt cơ mông ở điểm cao nhất;Hạ hông xuống chậm rãi."
                    },
                    new WorkoutPlan {
                        Name = "Plank nghiêng sườn (Side Plank)",
                        Type = "Strength",
                        DurationMinutes = 10,
                        CaloriesBurned = 60,
                        Difficulty = "Trung bình",
                        MuscleGroup = "Bụng",
                        Description = "Củng cố cơ liên sườn và cột sống hông khỏe khoắn.",
                        ImageUrl = "fa-slash",
                        Instructions = "Nằm nghiêng, chống khuỷu tay phải ngay dưới vai phải;Nâng hông lên sao cho cơ thể thẳng từ đầu đến chân;Giữ tay trái giơ thẳng lên trời;Đổi bên sau khi giữ từ 30-45 giây."
                    },
                    new WorkoutPlan {
                        Name = "Tư thế Squat tựa tường (Wall Sit)",
                        Type = "Strength",
                        DurationMinutes = 8,
                        CaloriesBurned = 50,
                        Difficulty = "Dễ",
                        MuscleGroup = "Mông/Đùi",
                        Description = "Bài tập tĩnh (isometric) giúp tăng sức bền cơ đùi trước cực kỳ mạnh mẽ.",
                        ImageUrl = "fa-warehouse",
                        Instructions = "Tựa lưng thẳng hoàn toàn vào tường;Trượt người xuống cho đến khi đùi song song mặt đất;Giữ gối vuông góc 90 độ;Hít thở đều và giữ tư thế lâu nhất có thể."
                    },
                    new WorkoutPlan {
                        Name = "Nằm sấp nâng tay chân (Superman)",
                        Type = "Strength",
                        DurationMinutes = 10,
                        CaloriesBurned = 70,
                        Difficulty = "Dễ",
                        MuscleGroup = "Lưng/Tay",
                        Description = "Củng cố cơ lưng dưới, cơ mông và cải thiện tình trạng gù lưng.",
                        ImageUrl = "fa-mask",
                        Instructions = "Nằm sấp, duỗi thẳng hai tay về phía trước và chân phía sau;Nâng đồng thời ngực, tay và chân lên khỏi thảm;Giữ nhịp căng cơ trong 2 giây;Hạ xuống nhẹ nhàng."
                    },

                    // FLEXIBILITY
                    new WorkoutPlan {
                        Name = "Yoga Chào Mặt Trời (Sun Salutation)",
                        Type = "Flexibility",
                        DurationMinutes = 20,
                        CaloriesBurned = 90,
                        Difficulty = "Dễ",
                        MuscleGroup = "Toàn thân",
                        Description = "Chuỗi động tác Yoga liên hoàn giúp đánh thức cơ thể và kéo giãn toàn bộ cơ gân kheo.",
                        ImageUrl = "fa-sun",
                        Instructions = "Đứng thẳng chắp tay trước ngực;Hít vào vươn tay lên cao ngửa người nhẹ;Thở ra gập người sâu tay chạm sàn;Hít vào đưa một chân ra sau tạo tư thế tấn;Chuyển qua chó úp mặt và lặp lại."
                    },
                    new WorkoutPlan {
                        Name = "Tư thế rắn hổ mang (Cobra Stretch)",
                        Type = "Flexibility",
                        DurationMinutes = 10,
                        CaloriesBurned = 40,
                        Difficulty = "Dễ",
                        MuscleGroup = "Bụng",
                        Description = "Động tác giãn cơ lưng và mở rộng ngực tối đa, giải tỏa đau mỏi thắt lưng.",
                        ImageUrl = "fa-otter",
                        Instructions = "Nằm sấp, úp hai lòng bàn tay dưới vai;Hít vào, nhấn tay đẩy ngực lên khỏi thảm;Giữ khuỷu tay hơi trùng nhẹ;Thả lỏng vai cách xa tai, hướng cằm lên nhẹ."
                    },
                    new WorkoutPlan {
                        Name = "Tư thế em bé (Child's Pose)",
                        Type = "Flexibility",
                        DurationMinutes = 8,
                        CaloriesBurned = 30,
                        Difficulty = "Dễ",
                        MuscleGroup = "Toàn thân",
                        Description = "Tư thế nghỉ ngơi, phục hồi sâu và giải tỏa áp lực cho vai và lưng dưới.",
                        ImageUrl = "fa-baby",
                        Instructions = "Quỳ trên thảm, ngồi lên gót chân;Gập người về trước, áp bụng lên đùi;Duỗi dài hai tay về phía trước thảm;Thả lỏng trán chạm thảm và hít thở sâu bụng."
                    },
                    new WorkoutPlan {
                        Name = "Giãn cơ đùi sau (Hamstring Stretch)",
                        Type = "Flexibility",
                        DurationMinutes = 10,
                        CaloriesBurned = 35,
                        Difficulty = "Dễ",
                        MuscleGroup = "Mông/Đùi",
                        Description = "Giảm căng tức cơ đùi sau sau khi đi bộ hoặc chạy bộ nhiều.",
                        ImageUrl = "fa-person-walking-arrow-right",
                        Instructions = "Ngồi trên thảm, duỗi một chân thẳng, co chân còn lại;Gập người từ hông về phía ngón chân thẳng;Giữ lưng thẳng, thở đều và thư giãn cơ đùi;Đổi chân sau 30 giây."
                    },
                    new WorkoutPlan {
                        Name = "Tư thế Con Mèo - Con Bò (Cat-Cow)",
                        Type = "Flexibility",
                        DurationMinutes = 10,
                        CaloriesBurned = 45,
                        Difficulty = "Dễ",
                        MuscleGroup = "Toàn thân",
                        Description = "Kéo giãn cột sống cổ, lưng và tăng tuần hoàn dịch khớp đốt sống lưng.",
                        ImageUrl = "fa-cat",
                        Instructions = "Chống hai tay và đầu gối trên thảm (tư thế cái bàn);Hít vào, võng lưng xuống, ngẩng đầu mắt nhìn lên (Bò);Thở ra, gù lưng cao lên, thu cằm sát ngực (Mèo);Thực hiện chậm theo hơi thở."
                    },
                    new WorkoutPlan {
                        Name = "Tư thế Chim bồ câu (Pigeon Pose)",
                        Type = "Flexibility",
                        DurationMinutes = 15,
                        CaloriesBurned = 60,
                        Difficulty = "Trung bình",
                        MuscleGroup = "Mông/Đùi",
                        Description = "Bài tập mở khớp hông sâu, giải phóng căng thẳng tâm lý bám ở hông.",
                        ImageUrl = "fa-dove",
                        Instructions = "Từ tư thế plank, đưa đầu gối phải ra sau cổ tay phải;Đặt cẳng chân phải chéo trên thảm;Trượt chân trái thẳng ra sau hết mức;Gập người nằm sấp trên chân phải thư giãn vai;Đổi bên."
                    },
                    new WorkoutPlan {
                        Name = "Tư thế vặn mình nằm ngửa (Supine Spinal Twist)",
                        Type = "Flexibility",
                        DurationMinutes = 10,
                        CaloriesBurned = 35,
                        Difficulty = "Dễ",
                        MuscleGroup = "Bụng",
                        Description = "Massage các cơ quan nội tạng và kéo giãn toàn bộ cột sống thắt lưng.",
                        ImageUrl = "fa-arrows-left-right",
                        Instructions = "Nằm ngửa, dang hai tay sang ngang tạo hình chữ T;Co đầu gối phải đặt chéo qua đùi trái;Nhấn gối phải hướng về sàn bên trái;Quay đầu nhìn sang bên phải;Đổi bên."
                    },
                    new WorkoutPlan {
                        Name = "Giãn cơ vai chéo (Shoulder Stretch)",
                        Type = "Flexibility",
                        DurationMinutes = 5,
                        CaloriesBurned = 20,
                        Difficulty = "Dễ",
                        MuscleGroup = "Ngực/Vai",
                        Description = "Giải tỏa sự co thắt của cơ vai deltoid do ngồi máy tính nhiều.",
                        ImageUrl = "fa-arrows-to-eye",
                        Instructions = "Đứng thẳng, đưa cánh tay phải chéo qua ngực;Dùng tay trái ép nhẹ cánh tay phải sát vào cơ thể;Giữ vai phải hạ thấp xa tai;Đổi bên sau 20 giây."
                    },
                    new WorkoutPlan {
                        Name = "Giãn cơ liên sườn đứng (Standing Side Bend)",
                        Type = "Flexibility",
                        DurationMinutes = 8,
                        CaloriesBurned = 30,
                        Difficulty = "Dễ",
                        MuscleGroup = "Bụng",
                        Description = "Mở rộng khung sườn, hỗ trợ phổi hô hấp sâu và kéo giãn cơ sườn.",
                        ImageUrl = "fa-up-down",
                        Instructions = "Đứng thẳng chân rộng hơn vai, hai tay giơ cao đan ngón;Nghiêng lườn người sang bên phải;Cảm nhận lực căng dọc mông sườn trái;Thở ra trở lại giữa và nghiêng bên trái."
                    },
                    new WorkoutPlan {
                        Name = "Tư thế chó úp mặt (Downward Dog)",
                        Type = "Flexibility",
                        DurationMinutes = 10,
                        CaloriesBurned = 50,
                        Difficulty = "Trung bình",
                        MuscleGroup = "Toàn thân",
                        Description = "Cải thiện lưu thông máu lên não, khỏe gân vai, lưng và kéo dài bắp chân.",
                        ImageUrl = "fa-paw",
                        Instructions = "Bắt đầu từ chống hai tay chân;Đẩy hông lên cao về sau tạo hình chữ V ngược;Ấn gót chân chạm sàn và đẩy mạnh vai từ sàn;Thả lỏng cổ đầu tự do."
                    }
                };

                context.WorkoutPlans.AddRange(workouts);
                context.SaveChanges();
            }
        }
        
        public static readonly List<(string Name, int Calories, double Fat, double Carbs, double Protein)> PredefinedFoods = new()
        {
            ("Phở bò", 350, 10.5, 45.2, 18.5),
            ("Phở gà", 310, 8.2, 42.5, 16.8),
            ("Bún chả", 450, 15.0, 55.0, 22.0),
            ("Bún bò Huế", 480, 16.5, 58.0, 24.5),
            ("Cơm tấm sườn bì chả", 620, 22.0, 80.0, 28.0),
            ("Bánh mì kẹp thịt", 380, 12.8, 48.0, 15.2),
            ("Gỏi cuốn tôm thịt (1 chiếc)", 60, 1.2, 8.5, 3.8),
            ("Bánh cuốn nhân thịt", 290, 8.5, 45.0, 9.5),
            ("Hủ tiếu Nam Vang", 400, 11.2, 58.5, 16.0),
            ("Cháo lòng", 320, 12.0, 38.0, 14.5),
            ("Cơm chiên Dương Châu", 520, 18.0, 72.0, 16.5),
            ("Trứng ốp la (1 quả)", 90, 7.0, 0.6, 6.3),
            ("Ức gà áp chảo (100g)", 165, 3.6, 0.0, 31.0),
            ("Thịt bò xào súp lơ (1 đĩa)", 280, 14.0, 12.5, 24.0),
            ("Cá thu kho tộ (100g)", 210, 11.5, 3.0, 22.5),
            ("Rau muống xào tỏi", 80, 5.0, 7.0, 2.2),
            ("Canh chua cá lóc (1 bát)", 120, 4.0, 10.0, 12.0),
            ("Đậu phụ sốt cà chua (1 đĩa)", 180, 10.0, 12.0, 11.5),
            ("Thịt ba chỉ luộc (100g)", 390, 35.0, 0.0, 16.0),
            ("Trứng cuộn hành", 110, 8.5, 1.2, 7.0),
            ("Salad ức gà sốt mè rang", 250, 12.0, 10.5, 25.0),
            ("Súp cua", 150, 4.5, 16.0, 11.5),
            ("Bánh canh cua", 380, 10.0, 52.0, 18.5),
            ("Bún riêu cua", 410, 12.5, 50.0, 17.0),
            ("Nem rán Việt Nam (1 chiếc)", 110, 6.5, 9.8, 3.2),
            ("Xôi xéo", 420, 11.0, 70.0, 9.8),
            ("Bún đậu mắm tôm đầy đủ", 550, 20.0, 65.0, 24.0),
            ("Cơm trắng (1 bát con)", 130, 0.3, 28.2, 2.7),
            ("Ngô ngọt luộc (1 bắp)", 150, 1.5, 32.0, 4.5),
            ("Khoai lang luộc (100g)", 90, 0.1, 21.0, 1.6),
            ("Quả bơ (100g)", 160, 14.7, 8.5, 2.0),
            ("Táo đỏ (1 quả trung bình)", 95, 0.3, 25.0, 0.5),
            ("Chuối tiêu (1 quả)", 105, 0.4, 27.0, 1.3),
            ("Cam sành (1 quả)", 60, 0.1, 15.0, 1.2),
            ("Dưa hấu (100g)", 30, 0.2, 7.5, 0.6),
            ("Sữa tươi không đường (110ml)", 70, 3.5, 5.0, 3.3),
            ("Sữa chua không đường (1 hộp)", 60, 3.0, 4.5, 3.5),
            ("Sữa chua nếp cẩm (1 hộp)", 120, 3.5, 18.0, 4.0),
            ("Hạt điều rang muối (30g)", 170, 13.5, 9.0, 5.5),
            ("Hạt hạnh nhân (30g)", 180, 15.0, 6.0, 6.5),
            ("Tôm hấp (100g)", 100, 1.0, 0.0, 22.0),
            ("Mực nướng (100g)", 110, 1.5, 1.2, 22.5),
            ("Chả lụa giò lụa (100g)", 180, 12.0, 2.0, 16.0),
            ("Bánh bao nhân thịt trứng cút", 350, 10.0, 52.0, 13.0),
            ("Sinh tố bơ (có sữa)", 280, 16.0, 32.0, 4.0),
            ("Cà phê sữa đá", 150, 4.5, 24.0, 2.5),
            ("Trà sữa trân châu truyền thống", 350, 12.0, 58.0, 3.0),
            ("Bánh flan (caramel)", 140, 5.0, 20.0, 4.5),
            ("Chè đỗ đen (1 cốc)", 220, 2.0, 45.0, 6.0),
            ("Canh khoai mỡ nấu thịt băm", 140, 5.0, 18.0, 8.0)
        };
    }
}
