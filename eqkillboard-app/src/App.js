import React, { Component } from 'react';
import { Query } from 'react-apollo';
import gql from 'graphql-tag';
import Killmails, { KillmailDataFragment} from './Killmails';
import logo from './logo.svg';
import './App.css';

const GET_KILLMAILS = gql`
  {
    allKillmails(orderBy: KILLED_AT_DESC) {
      nodes {
        ...KillmailData
      }    
    }
  }
  ${KillmailDataFragment}
`;

class App extends Component {
  render() {
    return (
      <div className="App">
        <header className="App-header">
          <img src={logo} className="App-logo" alt="logo" />
          <h1 className="App-title">Welcome to React</h1>
        </header>
        <p className="App-intro">
          To get started, edit <code>src/App.js</code> and save to reload.
        </p>
        <Query query={GET_KILLMAILS}>
          {({ loading, error, data}) => {
            if (loading) return 'Loading...';
            if (error) return `Error! ${error.message}`;
            
            return (
              <Killmails killmails={data.allKillmails.nodes} />
            );
          }}
        </Query>
      </div>
    );
  }
}

export default App;
