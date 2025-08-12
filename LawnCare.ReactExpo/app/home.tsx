import React from 'react';
import { View, StyleSheet, ScrollView } from 'react-native';
import { ThemedText } from '@/components/ThemedText';
import { ThemedView } from '@/components/ThemedView';
import { useAuth } from '@/contexts/AuthContext';
import { TouchableOpacity } from 'react-native';
import MenuBar from '@/components/MenuBar';

export default function HomeScreen() {
  const { user, logout } = useAuth();

  return (
    <ThemedView style={styles.container}>
      <MenuBar />
      <ScrollView style={styles.content}>
        <View style={styles.header}>
          <ThemedText type="title" style={styles.title}>
            Welcome to Lawn Care
          </ThemedText>
          {user?.name && (
            <ThemedText style={styles.greeting}>
              Hello, {user.name}!
            </ThemedText>
          )}
        </View>

        <View style={styles.section}>
          <ThemedText type="subtitle" style={styles.sectionTitle}>
            Quick Actions
          </ThemedText>
          <View style={styles.actionGrid}>
            <TouchableOpacity style={styles.actionCard}>
              <ThemedText style={styles.actionTitle}>Schedule Service</ThemedText>
              <ThemedText style={styles.actionDescription}>
                Book your next lawn care appointment
              </ThemedText>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard}>
              <ThemedText style={styles.actionTitle}>View Services</ThemedText>
              <ThemedText style={styles.actionDescription}>
                See all available lawn care services
              </ThemedText>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard}>
              <ThemedText style={styles.actionTitle}>Service History</ThemedText>
              <ThemedText style={styles.actionDescription}>
                View your past appointments
              </ThemedText>
            </TouchableOpacity>
            
            <TouchableOpacity style={styles.actionCard}>
              <ThemedText style={styles.actionTitle}>Account Settings</ThemedText>
              <ThemedText style={styles.actionDescription}>
                Manage your profile and preferences
              </ThemedText>
            </TouchableOpacity>
          </View>
        </View>

        <View style={styles.logoutSection}>
          <TouchableOpacity style={styles.logoutButton} onPress={logout}>
            <ThemedText style={styles.logoutButtonText}>Log Out</ThemedText>
          </TouchableOpacity>
        </View>
      </ScrollView>
    </ThemedView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  content: {
    flex: 1,
    padding: 20,
  },
  header: {
    marginBottom: 30,
    alignItems: 'center',
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    marginBottom: 10,
  },
  greeting: {
    fontSize: 18,
    opacity: 0.8,
  },
  section: {
    marginBottom: 30,
  },
  sectionTitle: {
    fontSize: 20,
    fontWeight: '600',
    marginBottom: 15,
  },
  actionGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-between',
  },
  actionCard: {
    width: '48%',
    backgroundColor: 'rgba(0, 122, 255, 0.1)',
    padding: 20,
    borderRadius: 12,
    marginBottom: 15,
  },
  actionTitle: {
    fontSize: 16,
    fontWeight: '600',
    marginBottom: 8,
  },
  actionDescription: {
    fontSize: 14,
    opacity: 0.7,
    lineHeight: 20,
  },
  logoutSection: {
    alignItems: 'center',
    marginTop: 20,
    marginBottom: 40,
  },
  logoutButton: {
    backgroundColor: '#FF3B30',
    paddingVertical: 12,
    paddingHorizontal: 30,
    borderRadius: 20,
  },
  logoutButtonText: {
    color: '#ffffff',
    fontSize: 16,
    fontWeight: '600',
  },
});