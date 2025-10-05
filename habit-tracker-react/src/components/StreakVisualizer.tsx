import { Box, Typography, Card, CardContent } from '@mui/material';
import { LocalFireDepartment, TrendingUp, EmojiEvents } from '@mui/icons-material';

interface StreakVisualizerProps {
  currentStreak: number;
  bestStreak: number;
  streakHistory: Array<{ startDate: string; endDate: string; length: number }>;
}

export default function StreakVisualizer({
  currentStreak,
  bestStreak,
  streakHistory,
}: StreakVisualizerProps) {
  // Get streak color based on length
  const getStreakColor = (streak: number): string => {
    if (streak >= 30) return '#DC2626'; // Red hot - 30+ days
    if (streak >= 14) return '#F59E0B'; // Orange - 2+ weeks
    if (streak >= 7) return '#10B981'; // Green - 1+ week
    if (streak >= 3) return '#3B82F6'; // Blue - 3+ days
    return '#6B7280'; // Gray - starting out
  };

  const getStreakMessage = (streak: number): string => {
    if (streak >= 30) return '🔥 On fire! Keep it up!';
    if (streak >= 14) return '💪 Strong streak!';
    if (streak >= 7) return '✨ One week done!';
    if (streak >= 3) return '🌱 Building momentum!';
    if (streak >= 1) return '🎯 Great start!';
    return '💡 Start your streak today!';
  };

  return (
    <Box>
      {/* Current Streak Highlight */}
      <Card
        sx={{
          background: `linear-gradient(135deg, ${getStreakColor(currentStreak)}15 0%, ${getStreakColor(
            currentStreak
          )}05 100%)`,
          border: 2,
          borderColor: getStreakColor(currentStreak),
          mb: 3,
        }}
      >
        <CardContent sx={{ textAlign: 'center', py: 4 }}>
          <LocalFireDepartment
            sx={{
              fontSize: 64,
              color: getStreakColor(currentStreak),
              mb: 2,
            }}
          />
          <Typography variant="h2" component="div" sx={{ color: getStreakColor(currentStreak), mb: 1 }}>
            {currentStreak}
          </Typography>
          <Typography variant="h6" color="text.secondary" gutterBottom>
            Day{currentStreak !== 1 ? 's' : ''} Current Streak
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
            {getStreakMessage(currentStreak)}
          </Typography>
        </CardContent>
      </Card>

      {/* Streak Stats Grid */}
      <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2, mb: 3 }}>
        {/* Best Streak */}
        <Card variant="outlined">
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
              <EmojiEvents sx={{ color: '#F59E0B' }} />
              <Typography variant="body2" color="text.secondary">
                Best Streak
              </Typography>
            </Box>
            <Typography variant="h4" component="div">
              {bestStreak}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              Personal record
            </Typography>
          </CardContent>
        </Card>

        {/* Progress to Beat Best */}
        <Card variant="outlined">
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
              <TrendingUp sx={{ color: '#3B82F6' }} />
              <Typography variant="body2" color="text.secondary">
                Progress
              </Typography>
            </Box>
            <Typography variant="h4" component="div">
              {bestStreak > 0 ? Math.round((currentStreak / bestStreak) * 100) : 0}%
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {currentStreak >= bestStreak
                ? 'New record!'
                : `${bestStreak - currentStreak} days to beat record`}
            </Typography>
          </CardContent>
        </Card>
      </Box>

      {/* Streak History */}
      {streakHistory.length > 0 && (
        <Box>
          <Typography variant="h6" gutterBottom sx={{ mt: 2 }}>
            Past Streaks
          </Typography>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
            {streakHistory.slice(0, 5).map((streak, index) => (
              <Card key={index} variant="outlined">
                <CardContent sx={{ py: 1.5, px: 2 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <LocalFireDepartment
                        sx={{
                          fontSize: 20,
                          color: getStreakColor(streak.length),
                        }}
                      />
                      <Typography variant="body2">
                        {streak.startDate} - {streak.endDate}
                      </Typography>
                    </Box>
                    <Box
                      sx={{
                        bgcolor: `${getStreakColor(streak.length)}20`,
                        color: getStreakColor(streak.length),
                        px: 2,
                        py: 0.5,
                        borderRadius: 1,
                        fontWeight: 600,
                      }}
                    >
                      {streak.length} days
                    </Box>
                  </Box>
                </CardContent>
              </Card>
            ))}
          </Box>
        </Box>
      )}

      {/* Milestones */}
      <Box sx={{ mt: 3 }}>
        <Typography variant="h6" gutterBottom>
          Streak Milestones
        </Typography>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
          {[
            { days: 3, label: 'Getting Started', icon: '🌱', achieved: currentStreak >= 3 },
            { days: 7, label: 'One Week Strong', icon: '✨', achieved: currentStreak >= 7 },
            { days: 14, label: 'Two Week Warrior', icon: '💪', achieved: currentStreak >= 14 },
            { days: 30, label: 'Monthly Master', icon: '🔥', achieved: currentStreak >= 30 },
            { days: 60, label: 'Unstoppable', icon: '🚀', achieved: currentStreak >= 60 },
            { days: 90, label: 'Legend', icon: '👑', achieved: currentStreak >= 90 },
          ].map((milestone) => (
            <Box
              key={milestone.days}
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                p: 1.5,
                borderRadius: 1,
                bgcolor: milestone.achieved ? 'success.light' : 'grey.100',
                opacity: milestone.achieved ? 1 : 0.5,
                transition: 'all 0.3s',
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Typography sx={{ fontSize: 24 }}>{milestone.icon}</Typography>
                <Box>
                  <Typography variant="body2" fontWeight={milestone.achieved ? 600 : 400}>
                    {milestone.label}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {milestone.days} days
                  </Typography>
                </Box>
              </Box>
              {milestone.achieved && (
                <Typography sx={{ color: 'success.main', fontWeight: 600 }}>✓ Unlocked</Typography>
              )}
            </Box>
          ))}
        </Box>
      </Box>
    </Box>
  );
}
