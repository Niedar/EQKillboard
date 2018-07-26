import React, { Component } from 'react';
import { Layout, Row, Col } from 'antd';
import Search from './Search';
import { Link } from 'react-router-dom'

const { Header, Footer, Sider, Content } = Layout;
class SiteHeader extends Component {
  render() {
    return (
        <Header>
          <Row>
            <Col xxl={4} xl={5} lg={8} md={8} sm={24} xs={24}>
              <Link to={"/"} style={{ textDecoration: "none"}}>
                <h1 style={{color: "white"}}>EQKillboard</h1>
              </Link>
            </Col>
            <Col xxl={20} xl={19} lg={16} md={16} sm={0} xs={0}>
              <Search />
            </Col>
          </Row>
        </Header>
    );
  }
}

export default SiteHeader;
