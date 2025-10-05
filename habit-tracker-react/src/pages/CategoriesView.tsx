import { useState } from 'react';
import {
  Box,
  Container,
  Typography,
  Button,
  Card,
  CardContent,
  CardActions,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Grid,
  Alert,
  CircularProgress,
  Chip,
} from '@mui/material';
import {
  Add,
  Edit,
  Delete,
  Category as CategoryIcon,
} from '@mui/icons-material';
import { useCategories, useCreateCategory, useUpdateCategory, useDeleteCategory } from '../hooks/useHabits';
import { useNotification } from '../contexts/NotificationContext';
import type { Category } from '../types/habit.types';

export default function CategoriesView() {
  const { showSuccess, showError } = useNotification();
  const { categories, isLoading } = useCategories();
  const { createCategoryAsync, isCreating } = useCreateCategory();
  const { updateCategoryAsync, isUpdating } = useUpdateCategory();
  const { deleteCategoryAsync, isDeleting } = useDeleteCategory();

  const [openDialog, setOpenDialog] = useState(false);
  const [editingCategory, setEditingCategory] = useState<Category | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<number | null>(null);

  // Form state
  const [formData, setFormData] = useState<Partial<Category>>({
    name: '',
    description: '',
    color: '#6366F1',
    icon: '',
  });

  const handleOpenNew = () => {
    setEditingCategory(null);
    setFormData({
      name: '',
      description: '',
      color: '#6366F1',
      icon: '',
    });
    setOpenDialog(true);
  };

  const handleEdit = (category: Category) => {
    setEditingCategory(category);
    setFormData({
      name: category.name,
      description: category.description || '',
      color: category.color || '#6366F1',
      icon: category.icon || '',
    });
    setOpenDialog(true);
  };

  const handleClose = () => {
    setOpenDialog(false);
    setEditingCategory(null);
    setFormData({
      name: '',
      description: '',
      color: '#6366F1',
      icon: '',
    });
  };

  const handleSave = async () => {
    try {
      if (!formData.name?.trim()) {
        showError('Category name is required');
        return;
      }

      if (editingCategory) {
        // Update existing category
        await updateCategoryAsync({ id: editingCategory.id, updates: formData });
      } else {
        // Create new category
        const newCategory: Category = {
          id: 0, // Will be assigned by sync service
          name: formData.name,
          description: formData.description,
          color: formData.color || '#6366F1',
          icon: formData.icon,
        };
        await createCategoryAsync(newCategory);
      }
      showSuccess('Category saved successfully');
      handleClose();
    } catch (error) {
      console.error('Failed to save category:', error);
      showError('Failed to save category. Please try again.');
    }
  };

  const handleDelete = async (id: number) => {
    try {
      await deleteCategoryAsync(id);
      showSuccess('Category deleted successfully');
      setDeleteConfirm(null);
    } catch (error) {
      console.error('Failed to delete category:', error);
      showError('Failed to delete category. It may be in use by habits.');
    }
  };

  const handleFormChange = (field: keyof Category, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  if (isLoading || isCreating || isUpdating || isDeleting) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4, textAlign: 'center' }}>
        <CircularProgress />
        <Typography sx={{ mt: 2 }}>
          {isLoading && 'Loading categories...'}
          {isCreating && 'Creating category...'}
          {isUpdating && 'Updating category...'}
          {isDeleting && 'Deleting category...'}
        </Typography>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <CategoryIcon sx={{ fontSize: 32 }} />
          <Typography variant="h4" component="h1">
            Categories
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<Add />}
          onClick={handleOpenNew}
          size="large"
        >
          Add Category
        </Button>
      </Box>

      {/* Categories Grid */}
      <Grid container spacing={3}>
        {categories.length === 0 && (
          <Grid size={{ xs: 12 }}>
            <Alert severity="info">
              No categories yet. Click "Add Category" to create your first category!
            </Alert>
          </Grid>
        )}
        
        {categories.map((category) => (
          <Grid size={{ xs: 12, sm: 6, md: 4 }} key={category.id}>
            <Card
              sx={{
                borderLeft: `6px solid ${category.color}`,
                height: '100%',
                display: 'flex',
                flexDirection: 'column',
              }}
            >
              <CardContent sx={{ flexGrow: 1 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  {category.icon && (
                    <Chip
                      label={category.icon}
                      size="small"
                      sx={{ fontSize: '1.2rem', fontFamily: 'emoji' }}
                    />
                  )}
                  <Typography variant="h6" component="h2">
                    {category.name}
                  </Typography>
                </Box>
                
                {category.description && (
                  <Typography variant="body2" color="text.secondary">
                    {category.description}
                  </Typography>
                )}

                {!category.description && (
                  <Typography variant="body2" color="text.disabled" fontStyle="italic">
                    No description
                  </Typography>
                )}
              </CardContent>
              
              <CardActions sx={{ justifyContent: 'flex-end', pt: 0 }}>
                <IconButton
                  size="small"
                  color="primary"
                  onClick={() => handleEdit(category)}
                  title="Edit category"
                >
                  <Edit />
                </IconButton>
                <IconButton
                  size="small"
                  color="error"
                  onClick={() => setDeleteConfirm(category.id)}
                  title="Delete category"
                >
                  <Delete />
                </IconButton>
              </CardActions>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Add/Edit Dialog */}
      <Dialog open={openDialog} onClose={handleClose} maxWidth="sm" fullWidth>
        <DialogTitle>
          {editingCategory ? 'Edit Category' : 'Add New Category'}
        </DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
            <TextField
              label="Name"
              required
              fullWidth
              value={formData.name}
              onChange={(e) => handleFormChange('name', e.target.value)}
              placeholder="e.g., Health, Productivity, Learning"
            />
            
            <TextField
              label="Description"
              fullWidth
              multiline
              rows={2}
              value={formData.description}
              onChange={(e) => handleFormChange('description', e.target.value)}
              placeholder="Optional description"
            />

            <Box sx={{ display: 'flex', gap: 2 }}>
              <TextField
                label="Color"
                type="color"
                fullWidth
                value={formData.color}
                onChange={(e) => handleFormChange('color', e.target.value)}
                InputLabelProps={{ shrink: true }}
              />
              
              <TextField
                label="Icon (Emoji)"
                fullWidth
                value={formData.icon}
                onChange={(e) => handleFormChange('icon', e.target.value)}
                placeholder="e.g., 💪 🧠 📚"
                inputProps={{ maxLength: 2 }}
              />
            </Box>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>Cancel</Button>
          <Button onClick={handleSave} variant="contained">
            {editingCategory ? 'Save Changes' : 'Create Category'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteConfirm !== null} onClose={() => setDeleteConfirm(null)}>
        <DialogTitle>Delete Category?</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete this category? 
            This action cannot be undone and may affect habits using this category.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteConfirm(null)}>Cancel</Button>
          <Button
            onClick={() => deleteConfirm && handleDelete(deleteConfirm)}
            variant="contained"
            color="error"
          >
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
}
