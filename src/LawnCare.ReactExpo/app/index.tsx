import { useEffect } from 'react';
import { Redirect } from 'expo-router';
import { useAuth } from '@/contexts/AuthContext';
import { View, ActivityIndicator } from 'react-native';

export default function IndexPage() {
  const { isAuthenticated, isLoading } = useAuth();
  
  console.log('🏠 Index Page - Auth State:', { isLoading, isAuthenticated });
  
  if (isLoading) {
    console.log('⏳ Index loading...');
    return (
      <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}>
        <ActivityIndicator size="large" />
      </View>
    );
  }
  
  if (isAuthenticated) {
    console.log('✅ Authenticated - redirecting to home');
    return <Redirect href="/home" />;
  }
  
  console.log('❌ Not authenticated - redirecting to login');
  return <Redirect href="/login" />;
}