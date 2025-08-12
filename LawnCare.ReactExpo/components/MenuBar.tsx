import React from 'react';
import { View, StyleSheet, TouchableOpacity, Alert } from 'react-native';
import { ThemedText } from '@/components/ThemedText';
import { useAuth } from '@/contexts/AuthContext';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

export default function MenuBar() {
  const { user } = useAuth();
  const insets = useSafeAreaInsets();

  const handleMenuPress = (menuItem: string) => {
    Alert.alert('Menu', `${menuItem} pressed - implement navigation here`);
  };

  return (
    <View style={[styles.container, { paddingTop: insets.top }]}>
      <View style={styles.menuBar}>
        <TouchableOpacity 
          style={styles.menuItem} 
          onPress={() => handleMenuPress('Home')}
        >
          <ThemedText style={styles.menuText}>Home</ThemedText>
        </TouchableOpacity>
        
        <TouchableOpacity 
          style={styles.menuItem} 
          onPress={() => handleMenuPress('Services')}
        >
          <ThemedText style={styles.menuText}>Services</ThemedText>
        </TouchableOpacity>
        
        <TouchableOpacity 
          style={styles.menuItem} 
          onPress={() => handleMenuPress('Schedule')}
        >
          <ThemedText style={styles.menuText}>Schedule</ThemedText>
        </TouchableOpacity>
        
        <View style={styles.userInfo}>
          <ThemedText style={styles.userText}>
            {user?.name?.split(' ')[0] || 'User'}
          </ThemedText>
        </View>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: '#007AFF',
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: 2,
    },
    shadowOpacity: 0.1,
    shadowRadius: 3.84,
    elevation: 5,
  },
  menuBar: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: 15,
    paddingVertical: 12,
  },
  menuItem: {
    paddingHorizontal: 15,
    paddingVertical: 8,
    marginRight: 10,
  },
  menuText: {
    color: '#ffffff',
    fontSize: 16,
    fontWeight: '600',
  },
  userInfo: {
    flex: 1,
    alignItems: 'flex-end',
  },
  userText: {
    color: '#ffffff',
    fontSize: 14,
    fontWeight: '500',
    opacity: 0.9,
  },
});