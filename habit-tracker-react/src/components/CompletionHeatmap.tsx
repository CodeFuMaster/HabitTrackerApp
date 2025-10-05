import { Box, Typography, Tooltip as MuiTooltip } from '@mui/material';
import { parseISO, format } from 'date-fns';

interface HeatmapDay {
  date: string;
  count: number;
  rate: number;
}

interface CompletionHeatmapProps {
  data: HeatmapDay[];
  maxCount: number;
}

export default function CompletionHeatmap({ data }: CompletionHeatmapProps) {
  // Group data by week
  const weeks: HeatmapDay[][] = [];
  let currentWeek: HeatmapDay[] = [];

  // Start from the first date and group by week (Sunday-Saturday)
  data.forEach((day, index) => {
    const date = parseISO(day.date);
    const dayOfWeek = date.getDay();

    // If it's Sunday and not the first day, start a new week
    if (dayOfWeek === 0 && currentWeek.length > 0) {
      weeks.push(currentWeek);
      currentWeek = [];
    }

    currentWeek.push(day);

    // If it's the last day, push the current week
    if (index === data.length - 1) {
      weeks.push(currentWeek);
    }
  });

  // Get color based on completion rate
  const getColor = (rate: number): string => {
    if (rate === 0) return '#EBEDF0'; // No completions
    if (rate < 25) return '#9BE9A8'; // 1-24%
    if (rate < 50) return '#40C463'; // 25-49%
    if (rate < 75) return '#30A14E'; // 50-74%
    return '#216E39'; // 75-100%
  };

  const dayLabels = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

  return (
    <Box>
      <Box sx={{ display: 'flex', gap: 0.5, overflow: 'auto' }}>
        {/* Day labels column */}
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5, pt: 3 }}>
          {dayLabels.map((label) => (
            <Box
              key={label}
              sx={{
                width: 30,
                height: 12,
                display: 'flex',
                alignItems: 'center',
                fontSize: '10px',
                color: 'text.secondary',
              }}
            >
              {label}
            </Box>
          ))}
        </Box>

        {/* Heatmap grid */}
        <Box sx={{ display: 'flex', gap: 0.5 }}>
          {weeks.map((week, weekIndex) => (
            <Box key={weekIndex} sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
              {/* Month label (show for first week of month) */}
              {week.length > 0 && (
                <Box
                  sx={{
                    height: 20,
                    display: 'flex',
                    alignItems: 'center',
                    fontSize: '10px',
                    color: 'text.secondary',
                  }}
                >
                  {weekIndex === 0 || parseISO(week[0].date).getDate() <= 7
                    ? format(parseISO(week[0].date), 'MMM')
                    : ''}
                </Box>
              )}

              {/* Week column - always render 7 cells */}
              {[0, 1, 2, 3, 4, 5, 6].map((dayIndex) => {
                const dayData = week.find((d) => parseISO(d.date).getDay() === dayIndex);

                if (!dayData) {
                  return (
                    <Box
                      key={dayIndex}
                      sx={{
                        width: 12,
                        height: 12,
                        bgcolor: 'transparent',
                        borderRadius: '2px',
                      }}
                    />
                  );
                }

                return (
                  <MuiTooltip
                    key={dayData.date}
                    title={
                      <Box>
                        <Typography variant="caption" display="block">
                          {format(parseISO(dayData.date), 'MMM dd, yyyy')}
                        </Typography>
                        <Typography variant="caption" display="block">
                          {dayData.count} completions ({dayData.rate}%)
                        </Typography>
                      </Box>
                    }
                    arrow
                  >
                    <Box
                      sx={{
                        width: 12,
                        height: 12,
                        bgcolor: getColor(dayData.rate),
                        borderRadius: '2px',
                        border: 1,
                        borderColor: 'divider',
                        cursor: 'pointer',
                        transition: 'transform 0.1s',
                        '&:hover': {
                          transform: 'scale(1.2)',
                          zIndex: 1,
                        },
                      }}
                    />
                  </MuiTooltip>
                );
              })}
            </Box>
          ))}
        </Box>
      </Box>

      {/* Legend */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 2, justifyContent: 'flex-end' }}>
        <Typography variant="caption" color="text.secondary">
          Less
        </Typography>
        {[0, 20, 40, 60, 80].map((rate) => (
          <Box
            key={rate}
            sx={{
              width: 12,
              height: 12,
              bgcolor: getColor(rate),
              borderRadius: '2px',
              border: 1,
              borderColor: 'divider',
            }}
          />
        ))}
        <Typography variant="caption" color="text.secondary">
          More
        </Typography>
      </Box>
    </Box>
  );
}
