import React, { createContext, useContext, useEffect, useState, ReactNode } from 'react';
import { Platform } from 'react-native';
import auth0 from '@/lib/auth0';

interface User {
  sub: string;
  name?: string;
  email?: string;
  picture?: string;
}

interface AuthContextType {
  user: User | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: () => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    checkAuthentication();
  }, []);

  const checkAuthentication = async () => {
    console.log('🔍 Checking authentication...');
    console.log('📱 Platform:', Platform.OS);
    
    // Skip Auth0 credential check on web due to known issues
    if (Platform.OS === 'web') {
      console.log('🌐 Web platform detected - skipping Auth0 credential check');
      setUser(null);
      setIsLoading(false);
      console.log('🔄 Loading complete (web)');
      return;
    }
    
    try {
      const credentials = await auth0.credentialsManager.getCredentials();
      console.log('🔑 Credentials:', credentials ? 'Found' : 'None');
      if (credentials?.accessToken) {
        console.log('✅ Access token found, getting user info...');
        const userInfo = await auth0.auth.userInfo({ token: credentials.accessToken });
        console.log('👤 User info:', userInfo);
        setUser(userInfo);
      } else {
        console.log('❌ No access token found');
        setUser(null);
      }
    } catch (error) {
      console.log('❌ No valid credentials found:', error);
      setUser(null);
    } finally {
      setIsLoading(false);
      console.log('🔄 Loading complete');
    }
  };

  const login = async () => {
    console.log('🔐 Starting login process...');
    console.log('📱 Platform:', Platform.OS);
    
    if (Platform.OS === 'web') {
      console.log('🌐 Web login - using Auth0 Universal Login');
      try {
        setIsLoading(true);
        
        // Build Auth0 Universal Login URL
        const domain = 'dev-vnezpmeetpxjkjm2.us.auth0.com';
        const clientId = 'nm08zZWbLtCYicU8l1AGmanwlWY8Cf0B';
        const redirectUri = encodeURIComponent(window.location.origin + '/home');
        const scope = encodeURIComponent('openid profile email');
        
        const authUrl = `https://${domain}/authorize?` +
          `response_type=code&` +
          `client_id=${clientId}&` +
          `redirect_uri=${redirectUri}&` +
          `scope=${scope}&` +
          `state=${Math.random().toString(36).substring(7)}`;
        
        console.log('🔗 Redirecting to Auth0:', authUrl);
        
        // Redirect to Auth0 Universal Login
        window.location.href = authUrl;
        
      } catch (error) {
        console.error('❌ Web login failed:', error);
        setIsLoading(false);
      }
      return;
    }
    
    try {
      setIsLoading(true);
      const credentials = await auth0.webAuth.authorize({
        scope: 'openid profile email',
      });
      
      await auth0.credentialsManager.saveCredentials(credentials);
      const userInfo = await auth0.auth.userInfo({ token: credentials.accessToken });
      setUser(userInfo);
    } catch (error) {
      console.error('Login failed:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async () => {
    try {
      setIsLoading(true);
      await auth0.webAuth.clearSession();
      await auth0.credentialsManager.clearCredentials();
      setUser(null);
    } catch (error) {
      console.error('Logout failed:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const value: AuthContextType = {
    user,
    isLoading,
    isAuthenticated: !!user,
    login,
    logout,
  };

  console.log('🚀 Auth State:', { isLoading, isAuthenticated: !!user, user: user?.name || 'None' });

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};