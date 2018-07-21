import React, { Component } from 'react';
import { Layout } from 'antd';
import KillmailsQuery from './KillmailsQuery';
import Killmails from './Killmails';
import logo from './logo.svg';
import './App.css';

const { Header, Footer, Sider, Content } = Layout;
class App extends Component {
  render() {
    return (
      <div style={{ width: '65%', margin: '0 auto', background: '#fafafa' }}>
        <Layout style={{ height: '100vh' }}>
          <Header><h1 style={{color: "white"}}>EQKillboard</h1></Header>
          <Layout>
            <Content>
              <KillmailsQuery>
                {({ loading, error, data}) => {
                  if (loading) return 'Loading...';
                  if (error) return `Error! ${error.message}`;
                  
                  return (
                    <Killmails killmails={data.allKillmails.nodes} />
                  );
                }}
              </KillmailsQuery>
            </Content>
            {/* <Sider></Sider> */}
          </Layout>
          <Footer></Footer>
        </Layout>
      </div>
    );
  }
}

export default App;
