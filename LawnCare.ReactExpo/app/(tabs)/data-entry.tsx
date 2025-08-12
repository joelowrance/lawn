import { StyleSheet, Alert } from 'react-native';
import { ThemedView } from '@/components/ThemedView';
import ServiceRequestForm from '@/components/ServiceRequestForm';
import { ServiceFormData } from '@/types/ServiceTypes';
import { EstimateService } from '@/services/EstimateService';

export default function DataEntryScreen() {
  const handleSave = async (data: ServiceFormData) => {
    const serviceRequest = {
      ...data,
      scheduledDate: data.scheduledDate?.toISOString() || new Date().toISOString(),
      userId: 'user123', // TODO: Get from auth context
      tenantId: '729C5768-6534-4C1D-815C-9E15613F1325', // TODO: Get from config
    };

    try {
      console.log('Sending Service Request:', JSON.stringify(serviceRequest, null, 2));
      
      const result = await EstimateService.createEstimate(serviceRequest);
      
      if (result.success) {
        Alert.alert(
          'Success',
          'Service request submitted successfully!',
          [{ text: 'OK' }]
        );
        console.log('API Response:', result.data);
      } else {
        Alert.alert(
          'Error',
          result.message || 'Failed to submit service request. Please try again.',
          [{ text: 'OK' }]
        );
        console.error('API Error:', result.message);
      }
    } catch (error) {
      console.error('Unexpected error:', error);
      Alert.alert(
        'Error',
        'An unexpected error occurred. Please try again.',
        [{ text: 'OK' }]
      );
    }
  };

  return (
    <ThemedView style={styles.container}>
      <ServiceRequestForm onSave={handleSave} />
    </ThemedView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
});