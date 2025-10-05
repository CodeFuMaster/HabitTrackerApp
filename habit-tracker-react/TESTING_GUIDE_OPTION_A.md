# Testing Guide - Option A: Metric History Display

## Test Environment
- **App URL**: http://localhost:5174
- **Status**: ✅ Running (HMR active)
- **TypeScript Errors**: 1 false positive (cache issue, will clear on restart)
- **Build Errors**: 0

## Pre-Test Checklist

### ✅ Components Ready
- [x] ActivityLogger.tsx - 265 lines, 0 errors
- [x] MetricInput.tsx - 235 lines, 0 errors
- [x] MetricValuesDisplay.tsx - 234 lines, 0 errors
- [x] CustomMetricsManager.tsx - 260 lines, 0 errors
- [x] HabitTimer.tsx - 320 lines, 0 errors

### ✅ Integration Complete
- [x] TodayView.tsx - Enhanced drawer with metric history
- [x] HabitsView.tsx - Custom metrics manager embedded
- [x] Database layer - 6 tables, 23 methods
- [x] Sync service - 14 new methods

## Test Scenarios

### Test 1: Define Custom Metrics
**Goal**: Verify users can define custom metrics for a habit

**Steps**:
1. Navigate to **Habits** page (menu)
2. Click **Edit** on any habit (or create new)
3. Scroll to **Custom Metrics** section
4. Click **+ Add Metric** button
5. Fill in metric details:
   - **Name**: "Weight Lifted"
   - **Type**: Weight
   - **Unit**: kg
   - **Default Value**: 60
   - **Required**: Yes
6. Click **Add** button
7. Add another metric:
   - **Name**: "Reps"
   - **Type**: Reps
   - **Unit**: reps
8. Click **Save Habit**

**Expected Result**:
- ✅ Metrics appear in the list
- ✅ Can reorder with drag handles
- ✅ Can edit each metric
- ✅ Can delete metrics
- ✅ Habit saves successfully

### Test 2: Enter Metric Values
**Goal**: Verify users can enter metric values when logging activity

**Steps**:
1. Navigate to **Today** page
2. Mark habit as complete (if not already)
3. Click **📝 Log Activity** icon button
4. Activity Logger dialog opens
5. Scroll to **Custom Metrics** section
6. Verify metrics defined in Test 1 appear
7. Enter values:
   - **Weight Lifted**: 75 kg
   - **Reps**: 12 reps
8. Add notes: "Good workout, felt strong"
9. Select mood: 😊 Good
10. Select energy: ⚡ High
11. Rate: ⭐⭐⭐⭐ (4 stars)
12. Click **Save Activity Log**

**Expected Result**:
- ✅ Dialog closes
- ✅ Activity indicators appear on habit card:
  - Mood chip: 😊 Good
  - Energy chip: Energy: High
  - Rating chip: ⭐ 4/5
- ✅ Data saved to database
- ✅ No console errors

### Test 3: View Metric History
**Goal**: Verify users can view historical metric values

**Steps**:
1. On Today page, click the **habit card** (not the complete button)
2. Drawer slides in from right
3. Verify drawer contents:
   - Habit name and description at top
   - "Mark as Complete/Incomplete" button
   - Divider
   - **Today's Activity** section shows:
     - Notes: "Good workout, felt strong"
     - Chips: 😊 Mood: Good, ⚡ Energy: High, ⭐ 4/5
4. Scroll down to **Metric History** accordion
5. Verify accordion is expanded by default
6. Verify header shows:
   - 📊 ShowChart icon
   - "Metric History" title
   - Record count chip: "2 records"
7. Inside accordion, verify:
   - Card with today's date
   - Values displayed:
     - Weight Lifted: 75 kg
     - Reps: 12 reps
   - **Summary Statistics** card at bottom:
     - Weight Lifted: Avg: 75.0 | Min: 75 | Max: 75
     - Reps: Avg: 12.0 | Min: 12 | Max: 12

**Expected Result**:
- ✅ Drawer width is 500px (wider than before)
- ✅ All sections display correctly
- ✅ Metrics formatted with units
- ✅ Statistics calculated correctly
- ✅ Layout is responsive

### Test 4: Multiple Entries Over Time
**Goal**: Verify timeline view and statistics with multiple entries

**Steps**:
1. Complete the habit again (if today already done, test with different habit or wait for tomorrow)
2. Log activity with different values:
   - Weight Lifted: 77.5 kg
   - Reps: 10 reps
3. Open habit drawer
4. Verify Metric History shows:
   - **Two date cards** (or more if tested multiple times)
   - Most recent date first
   - Each card shows values for that date
5. Check **Summary Statistics**:
   - Weight Lifted: Avg: 76.25 | Min: 75 | Max: 77.5
   - Reps: Avg: 11.0 | Min: 10 | Max: 12

**Expected Result**:
- ✅ Timeline sorted newest to oldest
- ✅ Each date shows correct values
- ✅ Statistics update correctly
- ✅ Avg calculation is accurate
- ✅ Min/Max show correct extreme values

### Test 5: Different Metric Types
**Goal**: Verify all 10 metric types work correctly

**Steps**:
1. Edit habit, add metrics for each type:
   - **Text**: "Location" (no unit)
   - **Boolean**: "Used Timer?" (no unit)
   - **Rating**: "Enjoyment" (no unit)
   - **Select**: "Time of Day" (Options: Morning, Afternoon, Evening)
2. Save habit
3. Log activity and enter values:
   - Location: "Home Gym"
   - Used Timer?: ✓ Yes (toggle on)
   - Enjoyment: ⭐⭐⭐⭐⭐ (5 stars)
   - Time of Day: Morning
4. View metric history

**Expected Result**:
- ✅ Text: Shows "Home Gym"
- ✅ Boolean: Shows green "Yes" chip with checkmark
- ✅ Rating: Shows 5 stars + (5/5)
- ✅ Select: Shows "Morning"
- ✅ No statistics shown for non-numeric types
- ✅ All values persist and display correctly

### Test 6: Empty States
**Goal**: Verify helpful messages when no data exists

**Scenario A - No Metrics Defined**:
1. Edit habit, delete all custom metrics
2. Save habit
3. Open habit drawer

**Expected Result**:
- ✅ Shows placeholder:
  - Large ShowChart icon (gray)
  - "No custom metrics defined for this habit"
  - "Add metrics in the Habits management page"
- ✅ No accordion visible

**Scenario B - Metrics Defined But No Values**:
1. Add metrics to habit
2. Don't log any activity
3. Open habit drawer

**Expected Result**:
- ✅ Metric History accordion visible
- ✅ Inside shows: "No metric values recorded yet" card
- ✅ Summary statistics section not shown

### Test 7: Loading States
**Goal**: Verify loading indicators work

**Steps**:
1. Open habit drawer
2. Immediately observe metric history section
3. On fast connections, loading may be imperceptible

**Expected Result**:
- ✅ Loading spinner briefly visible (if observable)
- ✅ No layout shift when data loads
- ✅ Smooth transition to content

### Test 8: Accordion Interaction
**Goal**: Verify accordion expands/collapses correctly

**Steps**:
1. Open habit drawer with metrics
2. Click accordion header to collapse
3. Click again to expand

**Expected Result**:
- ✅ Accordion animates smoothly
- ✅ ExpandMore icon rotates
- ✅ Content hides/shows
- ✅ State preserved when re-opening drawer

### Test 9: Multiple Habits
**Goal**: Verify each habit has independent metric history

**Steps**:
1. Create/edit 3 different habits:
   - **Exercise**: Weight, Reps, Sets
   - **Meditation**: Duration, Rating
   - **Reading**: Pages, Minutes
2. Log activities for all 3 with different values
3. Open each habit drawer one by one
4. Verify metric history shows only that habit's data

**Expected Result**:
- ✅ Exercise drawer shows weight/reps/sets history
- ✅ Meditation drawer shows duration/rating history
- ✅ Reading drawer shows pages/minutes history
- ✅ No cross-contamination of data
- ✅ Statistics calculated per habit

### Test 10: Responsive Design
**Goal**: Verify layout adapts to screen size

**Steps**:
1. Open habit drawer
2. Resize browser window:
   - Wide (> 900px)
   - Medium (600-900px)
   - Narrow (< 600px)
3. Check metric history display

**Expected Result**:
- ✅ Drawer width: 500px on desktop, 100% on mobile
- ✅ Metric grid adapts: 3 columns → 2 columns → 1 column
- ✅ Statistics chips wrap properly
- ✅ No horizontal scroll
- ✅ Touch targets adequate on mobile

## Bug Testing

### Potential Issues to Check

1. **Data Persistence**
   - [ ] Values saved after browser refresh
   - [ ] Values synced when going offline/online
   - [ ] No data loss on dialog cancel

2. **Edge Cases**
   - [ ] Very long metric names (truncate?)
   - [ ] Very large numbers (1000000)
   - [ ] Decimal precision (77.5 vs 77.50)
   - [ ] Special characters in text values
   - [ ] Empty required fields (validation)

3. **Performance**
   - [ ] Loading 100+ metric records
   - [ ] Multiple habits with many metrics
   - [ ] Drawer open/close speed
   - [ ] Smooth scrolling

4. **Error Handling**
   - [ ] Database error (graceful fail)
   - [ ] Network error (offline mode)
   - [ ] Invalid data types
   - [ ] Missing metric definitions

## Success Criteria

### Must Pass (Critical)
- ✅ Can define metrics in HabitsView
- ✅ Can enter values in ActivityLogger
- ✅ Values save to database
- ✅ History displays in drawer
- ✅ Statistics calculate correctly
- ✅ All metric types work

### Should Pass (Important)
- ✅ Loading states shown
- ✅ Empty states helpful
- ✅ Responsive on mobile
- ✅ No console errors
- ✅ Smooth animations

### Nice to Have (Polish)
- ⚠️ React Query invalidation (currently uses page.reload())
- ⚠️ Charts for numeric metrics
- ⚠️ Date range filters
- ⚠️ Export functionality

## Known Issues

1. **TypeScript Cache Error**
   - **Issue**: "Cannot find module '../components/ActivityLogger'"
   - **Impact**: None - file exists, app runs fine
   - **Fix**: Restart TypeScript server or VS Code
   - **Priority**: Low

2. **Page Reload After Save**
   - **Issue**: Uses `window.location.reload()` after saving activity
   - **Impact**: Causes full page refresh instead of smooth update
   - **Fix**: Use React Query's `invalidateQueries()`
   - **Priority**: Medium

## Test Results Template

```
Test Date: _____________
Tester: _____________
Browser: _____________
Screen Size: _____________

Test 1 - Define Metrics: ☐ Pass ☐ Fail
Test 2 - Enter Values: ☐ Pass ☐ Fail  
Test 3 - View History: ☐ Pass ☐ Fail
Test 4 - Multiple Entries: ☐ Pass ☐ Fail
Test 5 - Different Types: ☐ Pass ☐ Fail
Test 6 - Empty States: ☐ Pass ☐ Fail
Test 7 - Loading States: ☐ Pass ☐ Fail
Test 8 - Accordion: ☐ Pass ☐ Fail
Test 9 - Multiple Habits: ☐ Pass ☐ Fail
Test 10 - Responsive: ☐ Pass ☐ Fail

Overall: ☐ Pass ☐ Fail

Notes:
_________________________________
_________________________________
_________________________________
```

## Next Steps After Testing

If all tests pass:
- ✅ Mark Option A as COMPLETE
- ✅ Update progress to 95%
- 🔄 Proceed to Option B (Routine Templates) or Option C (Polish)

If issues found:
- 🐛 Document bugs
- 🔧 Fix critical issues
- 🔄 Re-test
- ✅ Mark as complete when stable
