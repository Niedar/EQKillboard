import React, { Component } from "react"
import { Query } from "react-apollo"
import gql from "graphql-tag"

export default class CharacterInfoQuery extends Component {
  render() {
    const { characterId, children } = this.props

    return (
      <Query query={GET_CHARACTERINFO} variables={{characterId}}>
        {result => children(result)}
      </Query>
    )
  }
}

const GET_CHARACTERINFO = gql`
query characterInfo($characterId: Int!) {
  characterById(id: $characterId) {
    id,
    name,
    level,
    guildByGuildId {
      id,
      name
    }
    classByClassId {
      id,
      name
    }
    killmailsByAttackerId {
      totalCount
    },
    killmailInvolvedsByAttackerId {
      totalCount
    },
    killmailsByVictimId {
      totalCount
    }
  }
  allCharacterRankedKillDeathInvolveds(condition: {id: $characterId}) {
    nodes {
      id
      name
      rankedKills
      rankedDeaths
    }
  }
}
`;
