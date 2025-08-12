import React, { useState } from 'react';
import {
  View,
  StyleSheet,
  ScrollView,
  TextInput,
  TouchableOpacity,
  Alert,
} from 'react-native';
import { ThemedText } from './ThemedText';
import { ThemedView } from './ThemedView';
import { ServiceFormData, Service } from '@/types/ServiceTypes';

interface ServiceRequestFormProps {
  onSave?: (data: ServiceFormData) => void;
}

export default function ServiceRequestForm({ onSave }: ServiceRequestFormProps) {
  const [formData, setFormData] = useState<ServiceFormData>({
    userId: '',
    tenantId: '',
    customerFirstName: '',
    customerLastName: '',
    customerAddress1: '',
    customerAddress2: '',
    customerAddress3: '',
    customerCity: '',
    customerState: '',
    customerZip: '',
    customerHomePhone: '',
    customerCellPhone: '',
    customerEmail: '',
    scheduledDate: null,
    estimatedCost: 0,
    estimatedDuration: 0,
    description: '',
    services: [],
  });

  const updateField = (field: keyof ServiceFormData, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const addService = () => {
    const newService: Service = {
      serviceName: '',
      quantity: 1,
      comment: '',
      price: 0,
    };
    setFormData(prev => ({
      ...prev,
      services: [...prev.services, newService],
    }));
  };

  const updateService = (index: number, field: keyof Service, value: any) => {
    setFormData(prev => ({
      ...prev,
      services: prev.services.map((service, i) =>
        i === index ? { ...service, [field]: value } : service
      ),
    }));
  };

  const removeService = (index: number) => {
    setFormData(prev => ({
      ...prev,
      services: prev.services.filter((_, i) => i !== index),
    }));
  };

  const calculateTotalCost = () => {
    return formData.services.reduce((total, service) => total + service.price, 0);
  };

  const handleSave = () => {
    const totalCost = calculateTotalCost();
    const dataToSave = {
      ...formData,
      estimatedCost: totalCost,
    };
    
    if (onSave) {
      onSave(dataToSave);
    } else {
      Alert.alert('Success', 'Service request saved!');
    }
  };

  return (
    <ScrollView style={styles.container}>
      <ThemedView style={styles.section}>
        <ThemedText type="subtitle" style={styles.sectionTitle}>
          Customer Information
        </ThemedText>
        
        <View style={styles.row}>
          <View style={styles.halfWidth}>
            <ThemedText style={styles.label}>First Name *</ThemedText>
            <TextInput
              style={styles.input}
              value={formData.customerFirstName}
              onChangeText={(text) => updateField('customerFirstName', text)}
              placeholder="Enter first name"
            />
          </View>
          <View style={styles.halfWidth}>
            <ThemedText style={styles.label}>Last Name *</ThemedText>
            <TextInput
              style={styles.input}
              value={formData.customerLastName}
              onChangeText={(text) => updateField('customerLastName', text)}
              placeholder="Enter last name"
            />
          </View>
        </View>

        <ThemedText style={styles.label}>Address Line 1 *</ThemedText>
        <TextInput
          style={styles.input}
          value={formData.customerAddress1}
          onChangeText={(text) => updateField('customerAddress1', text)}
          placeholder="Enter street address"
        />

        <ThemedText style={styles.label}>Address Line 2</ThemedText>
        <TextInput
          style={styles.input}
          value={formData.customerAddress2}
          onChangeText={(text) => updateField('customerAddress2', text)}
          placeholder="Apt, suite, etc. (optional)"
        />

        <ThemedText style={styles.label}>Address Line 3</ThemedText>
        <TextInput
          style={styles.input}
          value={formData.customerAddress3}
          onChangeText={(text) => updateField('customerAddress3', text)}
          placeholder="Additional address info (optional)"
        />

        <View style={styles.row}>
          <View style={styles.cityWidth}>
            <ThemedText style={styles.label}>City *</ThemedText>
            <TextInput
              style={styles.input}
              value={formData.customerCity}
              onChangeText={(text) => updateField('customerCity', text)}
              placeholder="City"
            />
          </View>
          <View style={styles.stateWidth}>
            <ThemedText style={styles.label}>State *</ThemedText>
            <TextInput
              style={styles.input}
              value={formData.customerState}
              onChangeText={(text) => updateField('customerState', text)}
              placeholder="CA"
              maxLength={2}
            />
          </View>
          <View style={styles.zipWidth}>
            <ThemedText style={styles.label}>ZIP *</ThemedText>
            <TextInput
              style={styles.input}
              value={formData.customerZip}
              onChangeText={(text) => updateField('customerZip', text)}
              placeholder="90210"
              keyboardType="numeric"
            />
          </View>
        </View>

        <View style={styles.row}>
          <View style={styles.halfWidth}>
            <ThemedText style={styles.label}>Home Phone</ThemedText>
            <TextInput
              style={styles.input}
              value={formData.customerHomePhone}
              onChangeText={(text) => updateField('customerHomePhone', text)}
              placeholder="(555) 123-4567"
              keyboardType="phone-pad"
            />
          </View>
          <View style={styles.halfWidth}>
            <ThemedText style={styles.label}>Cell Phone</ThemedText>
            <TextInput
              style={styles.input}
              value={formData.customerCellPhone}
              onChangeText={(text) => updateField('customerCellPhone', text)}
              placeholder="(555) 987-6543"
              keyboardType="phone-pad"
            />
          </View>
        </View>

        <ThemedText style={styles.label}>Email *</ThemedText>
        <TextInput
          style={styles.input}
          value={formData.customerEmail}
          onChangeText={(text) => updateField('customerEmail', text)}
          placeholder="john.doe@example.com"
          keyboardType="email-address"
          autoCapitalize="none"
        />
      </ThemedView>

      <ThemedView style={styles.section}>
        <ThemedText type="subtitle" style={styles.sectionTitle}>
          Service Details
        </ThemedText>

        <ThemedText style={styles.label}>Description</ThemedText>
        <TextInput
          style={[styles.input, styles.textArea]}
          value={formData.description}
          onChangeText={(text) => updateField('description', text)}
          placeholder="Describe the service request"
          multiline
          numberOfLines={3}
        />

        <ThemedText style={styles.label}>Estimated Duration (hours)</ThemedText>
        <TextInput
          style={styles.input}
          value={formData.estimatedDuration.toString()}
          onChangeText={(text) => updateField('estimatedDuration', parseFloat(text) || 0)}
          placeholder="2"
          keyboardType="numeric"
        />
      </ThemedView>

      <ThemedView style={styles.section}>
        <View style={styles.servicesHeader}>
          <ThemedText type="subtitle" style={styles.sectionTitle}>
            Services
          </ThemedText>
          <TouchableOpacity style={styles.addButton} onPress={addService}>
            <ThemedText style={styles.addButtonText}>+ Add Service</ThemedText>
          </TouchableOpacity>
        </View>

        {formData.services.map((service, index) => (
          <View key={index} style={styles.serviceCard}>
            <View style={styles.serviceHeader}>
              <ThemedText style={styles.serviceTitle}>Service {index + 1}</ThemedText>
              <TouchableOpacity
                style={styles.removeButton}
                onPress={() => removeService(index)}
              >
                <ThemedText style={styles.removeButtonText}>Remove</ThemedText>
              </TouchableOpacity>
            </View>

            <ThemedText style={styles.label}>Service Name *</ThemedText>
            <TextInput
              style={styles.input}
              value={service.serviceName}
              onChangeText={(text) => updateService(index, 'serviceName', text)}
              placeholder="e.g., Lawn Mowing"
            />

            <View style={styles.row}>
              <View style={styles.halfWidth}>
                <ThemedText style={styles.label}>Quantity</ThemedText>
                <TextInput
                  style={styles.input}
                  value={service.quantity.toString()}
                  onChangeText={(text) => updateService(index, 'quantity', parseInt(text) || 1)}
                  placeholder="1"
                  keyboardType="numeric"
                />
              </View>
              <View style={styles.halfWidth}>
                <ThemedText style={styles.label}>Price ($)</ThemedText>
                <TextInput
                  style={styles.input}
                  value={service.price.toString()}
                  onChangeText={(text) => updateService(index, 'price', parseFloat(text) || 0)}
                  placeholder="100.00"
                  keyboardType="numeric"
                />
              </View>
            </View>

            <ThemedText style={styles.label}>Comments</ThemedText>
            <TextInput
              style={styles.input}
              value={service.comment}
              onChangeText={(text) => updateService(index, 'comment', text)}
              placeholder="Additional notes about this service"
            />
          </View>
        ))}

        {formData.services.length === 0 && (
          <ThemedText style={styles.emptyText}>
            No services added yet. Tap "Add Service" to get started.
          </ThemedText>
        )}
      </ThemedView>

      <ThemedView style={styles.section}>
        <ThemedText type="subtitle" style={styles.sectionTitle}>
          Summary
        </ThemedText>
        <ThemedText style={styles.totalCost}>
          Total Estimated Cost: ${calculateTotalCost().toFixed(2)}
        </ThemedText>
      </ThemedView>

      <TouchableOpacity style={styles.saveButton} onPress={handleSave}>
        <ThemedText style={styles.saveButtonText}>Save Service Request</ThemedText>
      </TouchableOpacity>

      <View style={styles.bottomSpacer} />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f5f5f5',
  },
  section: {
    backgroundColor: '#ffffff',
    margin: 10,
    padding: 16,
    borderRadius: 8,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.1,
    shadowRadius: 2,
    elevation: 2,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: '600',
    marginBottom: 16,
  },
  label: {
    fontSize: 14,
    fontWeight: '500',
    marginBottom: 4,
    marginTop: 8,
  },
  input: {
    borderWidth: 1,
    borderColor: '#ddd',
    borderRadius: 6,
    padding: 12,
    fontSize: 16,
    backgroundColor: '#ffffff',
  },
  textArea: {
    height: 80,
    textAlignVertical: 'top',
  },
  row: {
    flexDirection: 'row',
    gap: 12,
  },
  halfWidth: {
    flex: 1,
  },
  cityWidth: {
    flex: 2,
  },
  stateWidth: {
    flex: 0.8,
  },
  zipWidth: {
    flex: 1,
  },
  servicesHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 16,
  },
  addButton: {
    backgroundColor: '#007AFF',
    paddingHorizontal: 16,
    paddingVertical: 8,
    borderRadius: 20,
  },
  addButtonText: {
    color: '#ffffff',
    fontSize: 14,
    fontWeight: '600',
  },
  serviceCard: {
    backgroundColor: '#f8f9fa',
    padding: 12,
    borderRadius: 6,
    marginBottom: 12,
    borderWidth: 1,
    borderColor: '#e9ecef',
  },
  serviceHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  serviceTitle: {
    fontSize: 16,
    fontWeight: '600',
  },
  removeButton: {
    backgroundColor: '#FF3B30',
    paddingHorizontal: 12,
    paddingVertical: 4,
    borderRadius: 12,
  },
  removeButtonText: {
    color: '#ffffff',
    fontSize: 12,
    fontWeight: '600',
  },
  emptyText: {
    textAlign: 'center',
    fontStyle: 'italic',
    color: '#666',
    marginVertical: 20,
  },
  totalCost: {
    fontSize: 18,
    fontWeight: '700',
    color: '#007AFF',
    textAlign: 'center',
  },
  saveButton: {
    backgroundColor: '#34C759',
    margin: 16,
    padding: 16,
    borderRadius: 8,
    alignItems: 'center',
  },
  saveButtonText: {
    color: '#ffffff',
    fontSize: 18,
    fontWeight: '600',
  },
  bottomSpacer: {
    height: 50,
  },
});