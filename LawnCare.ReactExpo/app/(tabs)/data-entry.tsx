import { StyleSheet, Alert } from 'react-native';
import { ThemedView } from '@/components/ThemedView';
import ServiceRequestForm from '@/components/ServiceRequestForm';
import { ServiceFormData } from '@/types/ServiceTypes';

export default function DataEntryScreen() {
  const handleSave = (data: ServiceFormData) => {
    const serviceRequest = {
      ...data,
      scheduledDate: data.scheduledDate?.toISOString() || new Date().toISOString(),
      userId: 'user123', // TODO: Get from auth context
      tenantId: '729C5768-6534-4C1D-815C-9E15613F1325', // TODO: Get from config
    };

    console.log('Service Request Data:', JSON.stringify(serviceRequest, null, 2));
    Alert.alert(
      'Success',
      'Service request saved successfully!',
      [{ text: 'OK' }]
    );
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