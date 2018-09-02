import React, { Component } from 'react';
import { Layout, Row, Col } from 'antd';
import Search from './Search';
import { Link } from 'react-router-dom'

const { Header } = Layout;
class SiteHeader extends Component {
  render() {
    return (
        <Header>
          <Row>
            <Col xxl={4} xl={6} lg={6} md={8} sm={12} xs={24}>
              <Link to={"/"} style={{ textDecoration: "none"}}>
                <h1 style={{color: "white"}}>EQKillboard</h1>
              </Link>
            </Col>
            <Col xxl={4} xl={6} lg={0} md={0} sm={0} xs={0}>
              <a href="https://riseofzek.com/"><img src="/rozlogo.png" style={{width: "150px"}} /></a>
            </Col>
            <Col xxl={16} xl={12} lg={18} md={16} sm={12} xs={0}>
              <Search />
            </Col>
          </Row>
        </Header>
    );
  }
}

export default SiteHeader;
