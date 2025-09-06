import { Service, ServiceFormData } from '@/types/ServiceTypes';
import React, { useState } from 'react';
import {
  Alert,
  ScrollView,
  StyleSheet,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { ThemedText } from './ThemedText';
import { ThemedView } from './ThemedView';

interface ServiceRequestFormProps {
  onSave?: (data: ServiceFormData) => void;
}

interface ValidationErrors {
  [key: string]: string;
}

export default function ServiceRequestForm({ onSave }: ServiceRequestFormProps) {
  const [formData, setFormData] = useState<ServiceFormData>({
    userId: '',
    tenantId: '',
    customerFirstName: 'Joe',
    customerLastName: 'Low',
    customerAddress1: '800 Main Street',
    customerAddress2: '',
    customerAddress3: '',
    customerCity: 'Anytown',
    customerState: 'MO',
    customerZip: '456891',
    customerHomePhone: '555-555-5555',
    customerCellPhone: '556-668-5566',
    customerEmail: 'foo@bar.com',
    scheduledDate: null,
    estimatedCost: 150,
    estimatedDuration: 1,
    description: 'Testing',
    services: [
      { serviceName: 'Lawn Mowing', quantity: 1, comment: '', price: 100 },
      { serviceName: 'Hedge Trimming', quantity: 1, comment: '', price: 50 }
    ],
  });

  const [validationErrors, setValidationErrors] = useState<ValidationErrors>({});

  const updateField = (field: keyof ServiceFormData, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // Clear validation error when user starts typing
    if (validationErrors[field]) {
      setValidationErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }
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
    // Clear services validation error when adding a service
    if (validationErrors.services) {
      setValidationErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors.services;
        return newErrors;
      });
    }
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

  const validateForm = (): boolean => {
    const errors: ValidationErrors = {};

    // Required fields validation
    if (!formData.customerFirstName.trim()) {
      errors.customerFirstName = 'First name is required';
    }
    if (!formData.customerLastName.trim()) {
      errors.customerLastName = 'Last name is required';
    }
    if (!formData.customerAddress1.trim()) {
      errors.customerAddress1 = 'Address is required';
    }
    if (!formData.customerCity.trim()) {
      errors.customerCity = 'City is required';
    }
    if (!formData.customerState.trim()) {
      errors.customerState = 'State is required';
    }
    if (!formData.customerZip.trim()) {
      errors.customerZip = 'ZIP code is required';
    }
    if (!formData.customerCellPhone.trim()) {
      errors.customerCellPhone = 'Cell phone is required';
    }
    if (!formData.customerEmail.trim()) {
      errors.customerEmail = 'Email is required';
    } else if (!/\S+@\S+\.\S+/.test(formData.customerEmail)) {
      errors.customerEmail = 'Please enter a valid email address';
    }

    // Services validation
    if (formData.services.length === 0) {
      errors.services = 'At least one service is required';
    } else {
      // Validate each service
      const servicesWithErrors = formData.services.some(service => !service.serviceName.trim());
      if (servicesWithErrors) {
        errors.services = 'All services must have a name';
      }
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSave = () => {
    if (!validateForm()) {
      Alert.alert('Validation Error', 'Please fill in all required fields and fix any errors.');
      return;
    }

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
              style={[styles.input, validationErrors.customerFirstName && styles.inputError]}
              value={formData.customerFirstName}
              onChangeText={(text) => updateField('customerFirstName', text)}
              placeholder="Enter first name"
            />
            {validationErrors.customerFirstName && (
              <ThemedText style={styles.errorText}>{validationErrors.customerFirstName}</ThemedText>
            )}
          </View>
          <View style={styles.halfWidth}>
            <ThemedText style={styles.label}>Last Name *</ThemedText>
            <TextInput
              style={[styles.input, validationErrors.customerLastName && styles.inputError]}
              value={formData.customerLastName}
              onChangeText={(text) => updateField('customerLastName', text)}
              placeholder="Enter last name"
            />
            {validationErrors.customerLastName && (
              <ThemedText style={styles.errorText}>{validationErrors.customerLastName}</ThemedText>
            )}
          </View>
        </View>

        <ThemedText style={styles.label}>Address Line 1 *</ThemedText>
        <TextInput
          style={[styles.input, validationErrors.customerAddress1 && styles.inputError]}
          value={formData.customerAddress1}
          onChangeText={(text) => updateField('customerAddress1', text)}
          placeholder="Enter street address"
        />
        {validationErrors.customerAddress1 && (
          <ThemedText style={styles.errorText}>{validationErrors.customerAddress1}</ThemedText>
        )}

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
              style={[styles.input, validationErrors.customerCity && styles.inputError]}
              value={formData.customerCity}
              onChangeText={(text) => updateField('customerCity', text)}
              placeholder="City"
            />
            {validationErrors.customerCity && (
              <ThemedText style={styles.errorText}>{validationErrors.customerCity}</ThemedText>
            )}
          </View>
          <View style={styles.stateWidth}>
            <ThemedText style={styles.label}>State *</ThemedText>
            <TextInput
              style={[styles.input, validationErrors.customerState && styles.inputError]}
              value={formData.customerState}
              onChangeText={(text) => updateField('customerState', text)}
              placeholder="CA"
              maxLength={2}
            />
            {validationErrors.customerState && (
              <ThemedText style={styles.errorText}>{validationErrors.customerState}</ThemedText>
            )}
          </View>
          <View style={styles.zipWidth}>
            <ThemedText style={styles.label}>ZIP *</ThemedText>
            <TextInput
              style={[styles.input, validationErrors.customerZip && styles.inputError]}
              value={formData.customerZip}
              onChangeText={(text) => updateField('customerZip', text)}
              placeholder="90210"
              keyboardType="numeric"
            />
            {validationErrors.customerZip && (
              <ThemedText style={styles.errorText}>{validationErrors.customerZip}</ThemedText>
            )}
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
            <ThemedText style={styles.label}>Cell Phone *</ThemedText>
            <TextInput
              style={[styles.input, validationErrors.customerCellPhone && styles.inputError]}
              value={formData.customerCellPhone}
              onChangeText={(text) => updateField('customerCellPhone', text)}
              placeholder="(555) 987-6543"
              keyboardType="phone-pad"
            />
            {validationErrors.customerCellPhone && (
              <ThemedText style={styles.errorText}>{validationErrors.customerCellPhone}</ThemedText>
            )}
          </View>
        </View>

        <ThemedText style={styles.label}>Email *</ThemedText>
        <TextInput
          style={[styles.input, validationErrors.customerEmail && styles.inputError]}
          value={formData.customerEmail}
          onChangeText={(text) => updateField('customerEmail', text)}
          placeholder="john.doe@example.com"
          keyboardType="email-address"
          autoCapitalize="none"
        />
        {validationErrors.customerEmail && (
          <ThemedText style={styles.errorText}>{validationErrors.customerEmail}</ThemedText>
        )}
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

        {validationErrors.services && (
          <ThemedText style={styles.errorText}>{validationErrors.services}</ThemedText>
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
  inputError: {
    borderColor: '#FF3B30',
    borderWidth: 2,
  },
  errorText: {
    color: '#FF3B30',
    fontSize: 12,
    marginTop: 4,
    marginBottom: 4,
  },
});