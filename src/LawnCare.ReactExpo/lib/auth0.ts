import Auth0 from 'react-native-auth0';

// Auth0 configuration - you'll need to replace these with your actual Auth0 credentials
const auth0 = new Auth0({
  domain: 'dev-vnezpmeetpxjkjm2.us.auth0.com', // e.g., 'your-domain.auth0.com'
  clientId: 'nm08zZWbLtCYicU8l1AGmanwlWY8Cf0B',
});

export default auth0;